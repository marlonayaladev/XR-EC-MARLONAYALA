// Etapa 4: segunda interacción por rayo. Cambia el color de un objeto
// usando MaterialPropertyBlock para NO modificar el material compartido
// del asset (el .mat en Assets/Materials queda intacto).
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace XRChallenge
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class CambiarColor : MonoBehaviour
    {
        [Tooltip("Renderer que cambiará de color. La asigna XRSceneBuilder por código.")]
        public Renderer objetivo;

        [Tooltip("Colores que se alternan al activar el botón.")]
        public Color[] paleta =
        {
            new Color(0.20f, 0.75f, 0.30f), // verde
            new Color(0.90f, 0.35f, 0.15f), // naranja
            new Color(0.25f, 0.45f, 0.95f), // azul
            new Color(0.85f, 0.20f, 0.60f), // magenta
        };

        MaterialPropertyBlock _bloque;
        XRSimpleInteractable _interactable;
        int _indice;

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
            AplicarSiguienteColor();
        }

        public void AplicarSiguienteColor()
        {
            if (objetivo == null || paleta == null || paleta.Length == 0)
                return;

            if (_bloque == null)
                _bloque = new MaterialPropertyBlock();

            objetivo.GetPropertyBlock(_bloque);
            _bloque.SetColor("_BaseColor", paleta[_indice]); // shader URP/Lit
            objetivo.SetPropertyBlock(_bloque);

            _indice = (_indice + 1) % paleta.Length;
        }
    }
}
