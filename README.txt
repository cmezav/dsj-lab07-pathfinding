LAB 07 - PATHFINDING | Proyecto inicial para Unity 6000.6.0f1

APERTURA
1. Descomprime todo el ZIP en una carpeta, por ejemplo Documents/DSJ_LAB07_Pathfinding_Unity6000.
2. Unity Hub > Add project from disk > selecciona la carpeta que contiene Assets, Packages y ProjectSettings.
3. Ábrelo con Unity 6000.6.0f1 y espera a que termine la importación y compilación.
4. Se creará y abrirá automáticamente Assets/Lab07/Scenes/Lab07_Inicio.unity.
   Si Unity deja abierta otra escena, usa Tools > DSJ Lab 07 > Open Starting Scene.

CONTENIDO INICIAL
- Proyecto 2D con cámara ortográfica y fondo oscuro.
- Environment: ocho paredes blancas, cada una con SpriteRenderer y BoxCollider2D.
- Las paredes están asignadas a User Layer 8: Environment.
- Player azul y Enemy naranja son marcadores provisionales para sustituir por los sprites de la guía.
- El Player tiene Rigidbody2D sin gravedad y CircleCollider2D.
- El objeto PathFinding es un marcador para añadir después AstarPath.
- La escena queda registrada en Build Settings.

SIGUIENTES PASOS DE LA GUÍA
- Importa el sprite del personaje y reemplaza el SpriteRenderer del Player y del Enemy.
- Añade el script de movimiento del Player.
- Descarga la versión actual gratuita de A* Pathfinding Project desde arongranberg.com/astar/download.
  Importa el paquete, agrega AstarPath al objeto PathFinding, crea Grid Graph 2D,
  selecciona Environment como Obstacles y pulsa Scan.
- Añade AIPath y AIDestinationSetter al Enemy y vincula el Player como destino.

NO se incluye la librería A* de terceros. La escena de paredes funciona sin ella.
No se reemplazará la escena si la editas después. Para abrirla de nuevo usa el menú Tools.
