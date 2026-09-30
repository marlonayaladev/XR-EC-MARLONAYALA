# XR Interaction Challenge — Escena `EC_XR_MARLONAYALA`


Proyecto Unity para el challenge de interacción XR del curso **Laboratorio de Realidad
Extendida (XR) para Videojuegos**, docente *Victor Alejandro Arroyo Castro*.

## Datos del entregable

| Dato | Valor |
|------|-------|
| Estudiante | Marlon Ayala |
| Código de estudiante | 2221899372 |
| Repositorio GitHub | [github.com/marlonayaladev/XR-EC-MARLONAYALA](https://github.com/marlonayaladev/XR-EC-MARLONAYALA) |
| Escena principal | `Assets/Scenes/EC_XR_MARLONAYALA.unity` |
| Versión de Unity | 6000.3.10f1 (URP) |


## Paquetes instalados (versiones reales de `Packages/manifest.json`)

| Paquete | Versión |
|---------|---------|
| `com.unity.xr.interaction.toolkit` | 3.6.1 |
| `com.unity.xr.management` | 4.7.0 |
| `com.unity.xr.openxr` | 1.18.0 |
| `com.unity.xr.hands` | 1.9.0 |
| `com.unity.inputsystem` | 1.18.0 |
| `com.unity.render-pipelines.universal` | 17.3.0 |
| `com.unity.ugui` | 2.0.0 |
| `com.unity.timeline` | 1.8.10 |

Samples importados: **Starter Assets** y **XR Device Simulator** (XRI 3.6.1).
OpenXR habilitado en *XR Plug-in Management* (Standalone) con perfiles
`OculusTouchControllerProfile` y `KHRSimpleControllerProfile`.

---

## Qué contiene la escena

- **Escenario**: piso 12 × 12 m con `BoxCollider`, 4 muros perimetrales, mesa con tablero,
  iluminación *Directional* + *Point* interactiva (`LuzInteractiva`).
- **Objetos manipulables** (`XRGrabInteractable`, `MovementType.VelocityTracking`, *Throw On Detach*):
  `CuboAgarre`, `EsferaAgarre`, `LlaveAgarre` — todos con `Rigidbody` y gravedad activa.
- **Interacción por rayo** (`XRSimpleInteractable`):
  - `Boton_Luz` → script `ToggleLuz.cs`: enciende/apaga la `LuzInteractiva`.
  - `Boton_Color` → script `CambiarColor.cs`: cambia el material de un objeto.
- **Teletransporte**: `TeleportationArea` en el piso (capa *Teleport* = bit 31) y
  `TeleportationProvider` en el rig XR Origin.
- **Contador** (`ContadorLuces.cs`): `Canvas` *World Space* con `TextMeshPro` que cuenta
  cuántas veces se encendió la luz (conectado a `ToggleLuz` por código).
- **XR Device Simulator** habilitado en la escena para probar en el Editor sin visor.

### Scripts propios (`Assets/Scripts/`)

| Script | Función |
|--------|---------|
| `ToggleLuz.cs` | Alterna la `Point Light`; notifica al contador |
| `CambiarColor.cs` | Cambia el color del material de un objeto |
| `ContadorLuces.cs` | Muestra el contador de activaciones en un `TextMeshPro` |

### Scripts de Editor (`Assets/Editor/`)

| Script | Función |
|--------|---------|
| `XRProjectConfigurator.cs` | Configura URP, XRI, OpenXR, TMP, samples (Etapa 1) |
| `XRSceneBuilder.cs` | Construye la escenaXR completa de forma idempotente + `Verify()` |

---

## Controles

### En el Editor (XR Device Simulator)

| Acción | Tecla / Input |
|--------|---------------|
| Mover cabeza | `W` / `S` (eje Z) · `A` / `D` (eje X) |
| Girar cabeza | Mouse (mantener clic derecho) |
| Interactuar (trigger) | Clic izquierdo del mouse |
| Agarrar / soltar (grip) | Tecla `G` (toggle) |
| Teletransportar | Thumbstick izquierdo hacia adelante + soltar |
| Subir / bajar | Thumbstick izquierdo hacia atrás + soltar |
| Rotar 45° | Thumbstick derecho |
| Activar/desactivar simulador | `Ctrl` + `F12` |

> Los ejes del Device Simulator se pueden invertir con `Ctrl` + `R` / `Ctrl` + `T`.

### En el visor (OpenXR / Meta Quest)

| Acción | Control |
|--------|---------|
| Apuntar | Mover la cabeza / los controladores |
| Agarrar y lanzar | **Grip** de cada controlador |
| Activar botón por rayo | **Trigger** (apuntar al botón) |
| Teletransportar | Thumbstick izquierdo hacia adelante |
| Rotar 45° | Thumbstick derecho |

---

## Capturas de pantalla

Las 3 capturas obligatorias van en la carpeta [`Capturas/`](Capturas/):

| Archivo | Contenido |
|---------|-----------|
| `general.png` | Vista general del escenario XR |
| `inspector.png` | Componentes XR en el Inspector |
| `interaccion.png` | Una interacción funcionando (rayo encendiendo la luz) |


---


## Cómo abrir y ejecutar

1. Abre el proyecto con **Unity 6000.3.10f1**.
2. Abre la escena `Assets/Scenes/EC_XR_MARLONAYALA.unity`.
3. Presiona **Play**. El XR Device Simulator aparece automáticamente.

### Verificación automática (opcional)

El script `XRSceneBuilder.Verify()` comprueba los criterios de aceptación del challenge:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe" `
  -batchmode -quit -nographics `
  -projectPath "C:\Users\AUTONOMA\My project (1)" `
  -executeMethod XRChallenge.XRSceneBuilder.Verify `
  -logFile verify.log
```

---

## Estructura del repositorio

```
Assets/
  Editor/              XRSceneBuilder.cs, XRProjectConfigurator.cs
  Materials/           Materiales URP/Lit
  Scenes/              EC_XR_MARLONAYALA.unity
  Scripts/             ToggleLuz.cs, CambiarColor.cs, ContadorLuces.cs
  XR/                  OpenXR Package Settings.asset
  XRI/Settings/        Configuración de XRI (capas, simulador)
Capturas/              Capturas de pantalla (PENDIENTE)
Packages/              manifest.json
ProjectSettings/       Configuración del proyecto
```
