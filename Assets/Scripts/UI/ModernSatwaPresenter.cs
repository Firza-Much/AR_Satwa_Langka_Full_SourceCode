using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SatwaLangka.Data;
using SatwaLangka.AR;

namespace SatwaLangka.UI
{
    public class ModernSatwaPresenter : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private AnimalDataSO currentData;

        [Header("UI Header")]
        [SerializeField] private TextMeshProUGUI txtAppTitle;
        [SerializeField] private Button btnHome;

        [Header("UI Card Elements")]
        [SerializeField] private TextMeshProUGUI txtCommonName;
        [SerializeField] private TextMeshProUGUI txtLatinName;
        [SerializeField] private TextMeshProUGUI txtStatusBadge;
        [SerializeField] private Image imgStatusBadge;
        [SerializeField] private TextMeshProUGUI txtBodyContent;

        [Header("Tab Buttons")]
        [SerializeField] private Button btnTabAbout;
        [SerializeField] private Button btnTabHabitat;
        [SerializeField] private Button btnTabFact;
        [SerializeField] private Image imgTabAbout;
        [SerializeField] private Image imgTabHabitat;
        [SerializeField] private Image imgTabFact;

        [Header("Action Buttons")]
        [SerializeField] private Button btnAudio;
        [SerializeField] private Button btnRotate;
        [SerializeField] private Button btnQuiz;

        [Header("3D Animal Presentation")]
        [SerializeField] private Transform animalStageRoot;
        [SerializeField] private GameObject currentSpawnedModel;

        private int activeTabIndex = 0; // 0 = About, 1 = Habitat, 2 = Fact
        private bool isAutoRotating = false;

        private void Start()
        {
            SetupButtonListeners();
            if (currentData != null)
            {
                DisplayAnimal(currentData);
            }
        }

        private void SetupButtonListeners()
        {
            if (btnTabAbout != null) btnTabAbout.onClick.AddListener(() => SwitchTab(0));
            if (btnTabHabitat != null) btnTabHabitat.onClick.AddListener(() => SwitchTab(1));
            if (btnTabFact != null) btnTabFact.onClick.AddListener(() => SwitchTab(2));
            if (btnRotate != null) btnRotate.onClick.AddListener(ToggleAutoRotate);
            if (btnAudio != null) btnAudio.onClick.AddListener(PlayAnimalAudio);
        }

        public void DisplayAnimal(AnimalDataSO data)
        {
            currentData = data;
            if (data == null) return;

            if (txtCommonName != null) txtCommonName.text = $"<b>{data.commonName}</b>";
            if (txtLatinName != null) txtLatinName.text = $"<i>{data.latinName}</i>";

            // Status Badge
            if (txtStatusBadge != null)
            {
                txtStatusBadge.text = $"<b>{GetStatusLabel(data.iucnStatus)}</b>";
            }
            if (imgStatusBadge != null)
            {
                imgStatusBadge.color = GetStatusColor(data.iucnStatus);
            }

            SwitchTab(0);
        }

        public void SwitchTab(int tabIndex)
        {
            activeTabIndex = tabIndex;
            if (currentData == null) return;

            // Highlight Tab Buttons
            Color activeColor = new Color(0.12f, 0.38f, 0.26f, 1.0f); // Forest Green Active
            Color inactiveColor = new Color(0.15f, 0.18f, 0.16f, 0.9f); // Slate Inactive

            if (imgTabAbout != null) imgTabAbout.color = tabIndex == 0 ? activeColor : inactiveColor;
            if (imgTabHabitat != null) imgTabHabitat.color = tabIndex == 1 ? activeColor : inactiveColor;
            if (imgTabFact != null) imgTabFact.color = tabIndex == 2 ? activeColor : inactiveColor;

            // Update Body Text
            if (txtBodyContent != null)
            {
                switch (tabIndex)
                {
                    case 0:
                        txtBodyContent.text = $"{currentData.description}\n\n<b>Pakan:</b> {currentData.diet}";
                        break;
                    case 1:
                        txtBodyContent.text = $"<b>Habitat Asli & Persebaran:</b>\n{currentData.habitat}\n\n<b>Kategori Satwa:</b> {currentData.category}";
                        break;
                    case 2:
                        txtBodyContent.text = $"<b>Fakta Unik:</b>\n{currentData.funFact}";
                        break;
                }
            }
        }

        private void ToggleAutoRotate()
        {
            isAutoRotating = !isAutoRotating;
        }

        private void Update()
        {
            if (isAutoRotating && currentSpawnedModel != null)
            {
                currentSpawnedModel.transform.Rotate(Vector3.up, 20f * Time.deltaTime, Space.World);
            }
        }

        private void PlayAnimalAudio()
        {
            Debug.Log($"Playing sound for {currentData?.commonName}");
        }

        private string GetStatusLabel(ConservationStatus status)
        {
            switch (status)
            {
                case ConservationStatus.CriticallyEndangered: return "Kritis (CR)";
                case ConservationStatus.Endangered: return "Terancam (EN)";
                case ConservationStatus.Vulnerable: return "Rentan (VU)";
                default: return "Risiko Rendah (LC)";
            }
        }

        private Color GetStatusColor(ConservationStatus status)
        {
            switch (status)
            {
                case ConservationStatus.CriticallyEndangered: return new Color(0.75f, 0.22f, 0.22f, 0.95f);
                case ConservationStatus.Endangered: return new Color(0.85f, 0.45f, 0.15f, 0.95f);
                case ConservationStatus.Vulnerable: return new Color(0.85f, 0.65f, 0.15f, 0.95f);
                default: return new Color(0.2f, 0.55f, 0.35f, 0.95f);
            }
        }
    }
}
