// Etapa 4: enciende/apaga la luz puntual cuando el jugador selecciona el
// botón con el rayo (XR Simple Interactable). El listener se conecta por
// código en OnEnable: no requiere configuración manual en el Inspector.
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace XRChallenge
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class ToggleLuz : MonoBehaviour
    {
        [Tooltip("Luz que se enciende y apaga. La asigna XRSceneBuilder por código.")]
        public Light luz;

        [Tooltip("Contador World Space opcional (suma cada vez que se enciende).")]
        public ContadorLuces contador;

        XRSimpleInteractable _interactable;

        void OnEnable()
        {
            _interactable = GetComponent<XRSimpleInteractable>();
            _interactable.selectEntered.AddListener(AlSeleccionar);
        }

        void OnDisable()
        {
            if (_interactable != null)
                _interactable.selectEntered.RemoveListener(AlSeleccionar);
        }

        // Evento selectEntered del XR Simple Interactable.
        void AlSeleccionar(SelectEnterEventArgs datos)
        {
            if (luz == null)
                return;

            luz.enabled = !luz.enabled;

            if (luz.enabled && contador != null)
                contador.SumarEncendido();
        }
    }
}
