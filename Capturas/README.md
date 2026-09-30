# Capturas de pantalla — XR Interaction Challenge

Carpeta destinada a las 3 capturas obligatorias del README. Coloca aquí los archivos
con **exactamente** estos nombres (formato PNG):

| Archivo         | Contenido requerido                                                              |
|-----------------|----------------------------------------------------------------------------------|
| `general.png`   | Vista general del escenario XR (piso, paredes, mesa, objetos con materiales)      |
| `inspector.png` | Componentes XR en el Inspector (XR Grab Interactable o XR Origin / OpenXR)        |
| `interaccion.png` | Una interacción funcionando (ej. el rayo encendiendo la Point Light "Boton_Luz") |

## Estado

- [ ] PENDIENTE: captura `general.png`
- [ ] PENDIENTE: captura `inspector.png`
- [ ] PENDIENTE: captura `interaccion.png`

## Cómo tomarlas (sugerido)

1. Abre el proyecto en Unity 6000.3.10f1 y abre la escena `Assets/Scenes/EC_XR_MARLONAYALA.unity`.
2. **general.png**: coloca la Scene View mostrando toda la sala y haz captura.
3. **inspector.png**: selecciona el objeto `CuboAgarre` (o `EsferaAgarre`) y muestra el
   Inspector con `XR Grab Interactable`, o selecciona el `XROrigin` con `NearFarInteractor`.
4. **interaccion.png**: en Play Mode con el XR Device Simulator, apunta el rayo al
   objeto `Boton_Luz` y captura el instante en que la Point Light se enciende.

> Estos PNG no se generan por código: el usuario debe pegarlos antes de subir el repo.
