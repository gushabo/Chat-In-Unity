import socket
import threading
import re
import os

HOST = 'localhost'
PORT = 8080
BUFFER = 1024
ROOM_CAPACITY = 5   # límite por sala

# clientes: {conn: {"name": str, "addr": (ip,port), "room": str|None}}
clientes = {}
# rooms: {room_name: set(conns)}
rooms = {}

def limpiar_texto(t):
    t = t.replace('\ufeff', '')
    t = re.sub(r'[\r\n]+', ' ', t)
    return t.strip()

def broadcast_room(room, mensaje, emisor=None):
    """Envía a todos en la sala (excepto emisor si se pasa)."""
    conns = rooms.get(room, set()).copy()
    for c in conns:
        try:
            if emisor is None or c != emisor:
                c.sendall((mensaje + "\n").encode('utf-8'))
        except:
            c.close()
            salir_de_todo(c)

def lista_rooms_str():
    if not rooms:
        return ">>> Rooms: (vacío)"
    data = []
    for r, members in rooms.items():
        data.append(f"{r} ({len(members)}/{ROOM_CAPACITY})")
    return ">>> Rooms: " + ", ".join(sorted(data))

def enviar_lista_usuarios_room(room):
    if room is None: 
        return
    nombres = []
    for c in rooms.get(room, set()):
        info = clientes.get(c)
        if info: 
            nombres.append(info["name"])
    msg = ">>> Usuarios conectados: " + ", ".join(sorted(nombres))
    broadcast_room(room, msg)

def entrar_room(conn, room):
    info = clientes.get(conn)
    if not info:
        return
    prev = info["room"]
    if prev == room:
        conn.sendall(f">>> Ya estás en '{room}'\n".encode('utf-8'))
        return

    # salir de sala previa (igual que ya tenías)
    if prev and prev in rooms and conn in rooms[prev]:
        rooms[prev].remove(conn)
        broadcast_room(prev, f">>> {info['name']} salió del room '{prev}'", emisor=conn)
        enviar_lista_usuarios_room(prev)
        if not rooms[prev]:
            del rooms[prev]

    # ✅ verificar cupo antes de entrar
    actuales = len(rooms.get(room, set()))
    if actuales >= ROOM_CAPACITY:
        conn.sendall(f">>> Room '{room}' está lleno ({actuales}/{ROOM_CAPACITY}).\n".encode('utf-8'))
        return

    # entrar a nueva
    rooms.setdefault(room, set()).add(conn)
    info["room"] = room
    conn.sendall(f">>> Entraste al room '{room}' ({actuales+1}/{ROOM_CAPACITY})\n".encode('utf-8'))
    broadcast_room(room, f">>> {info['name']} se unió al room '{room}'", emisor=conn)
    enviar_lista_usuarios_room(room)

def salir_de_room(conn):
    info = clientes.get(conn)
    if not info: 
        return
    room = info["room"]
    if not room: 
        return
    if room in rooms and conn in rooms[room]:
        rooms[room].remove(conn)
        broadcast_room(room, f">>> {info['name']} salió del room '{room}'", emisor=conn)
        enviar_lista_usuarios_room(room)
        if not rooms[room]:
            del rooms[room]
    info["room"] = None

def salir_de_todo(conn):
    info = clientes.pop(conn, None)
    if not info: 
        return
    room = info.get("room")
    if room and room in rooms and conn in rooms[room]:
        rooms[room].remove(conn)
        broadcast_room(room, f">>> {info['name']} salió del chat", emisor=conn)
        enviar_lista_usuarios_room(room)
        if not rooms[room]:
            del rooms[room]

def mostrar_estado():
    os.system('cls' if os.name == 'nt' else 'clear')
    print("=== 🖥️ ESTADO DEL SERVIDOR DE CHAT ===\n")
    if clientes:
        print("👥 Conectados:")
        for c, inf in clientes.items():
            ip, prt = inf["addr"]
            print(f"   - {inf['name']} @ {ip}:{prt}  room={inf['room']}")
    else:
        print("⚠️  No hay usuarios conectados.")
    print("\n" + lista_rooms_str())
    print("\n======================================\n")

def manejar_cliente(conn, addr):
    print(f"[+] Conexión entrante desde {addr}")
    try:
        # nombre por línea
        reader = conn.makefile('r', encoding='utf-8')
        nombre = limpiar_texto(reader.readline().strip())
        if not nombre:
            conn.close()
            print(f"[!] {addr} sin nombre; conexión cerrada.")
            return
        # nombre único
        if any(inf["name"] == nombre for inf in clientes.values()):
            conn.sendall(f">>> El nombre '{nombre}' ya está en uso.\n".encode('utf-8'))
            conn.close()
            print(f"[!] Duplicado rechazado: {nombre}")
            return

        clientes[conn] = {"name": nombre, "addr": addr, "room": None}
        conn.sendall(f">>> Conectado como {nombre}\n".encode('utf-8'))
        conn.sendall((lista_rooms_str() + "\n").encode('utf-8'))
        mostrar_estado()

        while True:
            data = conn.recv(BUFFER)
            if not data:
                break
            msg = limpiar_texto(data.decode('utf-8-sig', errors='replace'))
            if not msg:
                continue

            info = clientes.get(conn)
            user = info["name"] if info else "Desconocido"
            room = info["room"] if info else None

            # comandos
            if msg.lower() == "/salir":
                break
            if msg.lower() == "/rooms":
                conn.sendall((lista_rooms_str() + "\n").encode('utf-8'))
                continue
            if msg.lower().startswith("/join "):
                r = limpiar_texto(msg[6:])
                if not r:
                    conn.sendall(b">>> Uso: /join NOMBRE_SALA\n")
                else:
                    entrar_room(conn, r)
                    mostrar_estado()
                continue
            if msg.lower() == "/leave":
                salir_de_room(conn)
                mostrar_estado()
                continue
            if msg.startswith("/msg "):
                partes = msg.split(" ", 2)
                if len(partes) < 3:
                    conn.sendall(b">>> Uso: /msg NOMBRE mensaje\n")
                else:
                    dest, txt = partes[1], partes[2]
                    # privado ignora room
                    ok = False
                    for c2, inf2 in clientes.items():
                        if inf2["name"] == dest:
                            c2.sendall(f"[Privado de {user}] {txt}\n".encode('utf-8'))
                            ok = True
                            break
                    if ok:
                        conn.sendall(f"[Privado a {dest}] {txt}\n".encode('utf-8'))
                    else:
                        conn.sendall(f">>> No se encontró a '{dest}'\n".encode('utf-8'))
                continue

            # mensaje público -> solo a la sala actual
            if room is None:
                conn.sendall((">>> No estás en ninguna sala. Usa /join NOMBRE" + "\n").encode("utf-8"))
            else:
                print(f"💬 [{user} @ {room}] {msg}")
                broadcast_room(room, f"[{user}] {msg}", emisor=conn)

    except ConnectionResetError:
        print(f"[!] {addr} cerró conexión inesperadamente.")
    finally:
        salir_de_todo(conn)
        conn.close()
        mostrar_estado()

def main():
    srv = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    srv.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    srv.bind((HOST, PORT))
    srv.listen(16)
    print(f"Servidor en {HOST}:{PORT}")
    mostrar_estado()
    try:
        while True:
            c, a = srv.accept()
            threading.Thread(target=manejar_cliente, args=(c, a), daemon=True).start()
    except KeyboardInterrupt:
        print("\nServidor detenido.")
    finally:
        srv.close()

if __name__ == "__main__":
    main()
