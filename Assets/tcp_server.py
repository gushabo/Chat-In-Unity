import socket
import threading
import re
import os

HOST = 'localhost'
PORT = 8080
BUFFER = 1024
clientes = {}  # {conn: (nombre, addr)}

# --- Limpieza de texto ---
def limpiar_texto(texto):
    texto = texto.replace('\ufeff', '')  # eliminar BOM
    texto = re.sub(r'[\r\n]+', ' ', texto)
    return texto.strip()

# --- Enviar mensaje a todos (menos al emisor) ---
def broadcast(mensaje, emisor=None):
    for conn in list(clientes.keys()):
        try:
            if conn != emisor:
                conn.sendall((mensaje + "\n").encode('utf-8'))
        except:
            conn.close()
            clientes.pop(conn, None)

# --- Enviar privado ---
def enviar_privado(remitente, destinatario, mensaje):
    for conn, (nombre, _) in clientes.items():
        if nombre == destinatario:
            try:
                conn.sendall(f"[Privado de {remitente}] {mensaje}\n".encode('utf-8'))
            except:
                conn.close()
                clientes.pop(conn, None)
            return True
    return False

# --- Enviar lista de usuarios ---
def enviar_lista_usuarios():
    if not clientes:
        return
    lista = ", ".join(nombre for _, (nombre, _) in clientes.items())
    broadcast(f">>> Usuarios conectados: {lista}")
    mostrar_estado_servidor()

# --- Mostrar estado ---
def mostrar_estado_servidor():
    os.system('cls' if os.name == 'nt' else 'clear')
    print("=== 🖥️ ESTADO DEL SERVIDOR DE CHAT ===\n")
    if clientes:
        print("👥 Usuarios conectados:")
        for _, (nombre, addr) in clientes.items():
            print(f"   - {nombre} ({addr[0]}:{addr[1]})")
    else:
        print("⚠️  No hay usuarios conectados.")
    print("\n======================================\n")

# --- Manejar cliente ---
def manejar_cliente(conn, addr):
    print(f"[+] Conexión entrante desde {addr}")
    try:
        # 🔹 Leer nombre correctamente
        reader = conn.makefile('r', encoding='utf-8')
        nombre = reader.readline().strip()
        nombre = limpiar_texto(nombre)

        if not nombre:
            conn.close()
            print(f"[!] Cliente {addr} no envió nombre, cerrado.")
            return

        # Evitar duplicados
        if any(n == nombre for _, (n, _) in clientes.items()):
            conn.sendall(f">>> El nombre '{nombre}' ya está en uso.\n".encode('utf-8'))
            conn.close()
            print(f"[!] Nombre duplicado rechazado: {nombre}")
            return

        clientes[conn] = (nombre, addr)
        print(f"🟢 {nombre} ({addr[0]}:{addr[1]}) conectado")

        conn.sendall(f">>> Conectado como {nombre}\n".encode('utf-8'))
        broadcast(f">>> {nombre} se ha unido al chat", emisor=conn)
        enviar_lista_usuarios()

        while True:
            data = conn.recv(BUFFER)
            if not data:
                break

            mensaje = data.decode('utf-8-sig', errors='replace')
            mensaje = limpiar_texto(mensaje)
            if not mensaje:
                continue

            if mensaje.lower() == "/salir":
                break

            if mensaje.startswith("/msg"):
                partes = mensaje.split(" ", 2)
                if len(partes) < 3:
                    conn.sendall(b">>> Uso: /msg NOMBRE mensaje\n")
                else:
                    destinatario, texto = partes[1], partes[2]
                    if enviar_privado(nombre, destinatario, texto):
                        conn.sendall(f"[Privado a {destinatario}] {texto}\n".encode('utf-8'))
                    else:
                        conn.sendall(f">>> No se encontró a '{destinatario}'\n".encode('utf-8'))
            else:
                print(f"💬 [{nombre}] {mensaje}")
                broadcast(f"[{nombre}] {mensaje}", emisor=conn)

    except ConnectionResetError:
        print(f"[!] {addr} cerró la conexión inesperadamente.")
    finally:
        nombre, _ = clientes.pop(conn, ("Desconocido", addr))
        conn.close()
        print(f"🔴 {nombre} ({addr}) desconectado")
        broadcast(f">>> {nombre} salió del chat")
        enviar_lista_usuarios()

# --- Principal ---
def main():
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server.bind((HOST, PORT))
    server.listen(5)

    print(f"Servidor de chat en {HOST}:{PORT}")
    mostrar_estado_servidor()

    try:
        while True:
            conn, addr = server.accept()
            threading.Thread(target=manejar_cliente, args=(conn, addr), daemon=True).start()
    except KeyboardInterrupt:
        print("\nServidor detenido manualmente.")
    finally:
        server.close()

if __name__ == "__main__":
    main()
