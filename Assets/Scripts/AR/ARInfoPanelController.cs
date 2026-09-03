using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SatwaLangka.Data;

namespace SatwaLangka.AR
{
    public class ARInfoPanelController : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] public TextMeshProUGUI txtCommonName;
        [SerializeField] public TextMeshProUGUI txtLatinName;
        [SerializeField] public TextMeshProUGUI txtStatusBadge;
        [SerializeField] public Image imgStatusBadge;

        [Header("Tabs")]
        [SerializeField] public Button tabDeskripsi;
        [SerializeField] public Button tabHabitat;
        [SerializeField] public Button tabMitigasi;
        [SerializeField] public Button tabFakta;
        [SerializeField] public Image imgTabDeskripsi;
        [SerializeField] public Image imgTabHabitat;
        [SerializeField] public Image imgTabMitigasi;
        [SerializeField] public Image imgTabFakta;

        [Header("Content")]
        [SerializeField] public TextMeshProUGUI txtContent;
        [SerializeField] public Button btnPlayAudio;
        [SerializeField] public AudioSource audioSource;

        [Header("Toggle")]
        [SerializeField] public Button btnTogglePanel;
        [SerializeField] public GameObject panelContent;

        private AnimalDataSO currentAnimal;
        private int currentTab = 0;
        private bool isPanelExpanded = true;

        private static readonly Color TabActiveColor   = new Color(0.055f, 0.478f, 0.353f, 1f);
        private static readonly Color TabInactiveColor = new Color(0.2f,   0.2f,   0.2f,   1f);
        private static readonly Color ColorCR = new Color(0.85f, 0.2f,  0.2f,  1f);
        private static readonly Color ColorEN = new Color(0.85f, 0.55f, 0.1f,  1f);
        private static readonly Color ColorVU = new Color(0.8f,  0.75f, 0.1f,  1f);
        private static readonly Color ColorLC = new Color(0.2f,  0.65f, 0.3f,  1f);

        private void Start()
        {
            if (tabDeskripsi   != null) tabDeskripsi.onClick.AddListener(   () => ShowTab(0));
            if (tabHabitat     != null) tabHabitat.onClick.AddListener(     () => ShowTab(1));
            if (tabMitigasi    != null) tabMitigasi.onClick.AddListener(    () => ShowTab(2));
            if (tabFakta       != null) tabFakta.onClick.AddListener(       () => ShowTab(3));
            if (btnPlayAudio   != null) btnPlayAudio.onClick.AddListener(PlayAudio);
            if (btnTogglePanel != null) btnTogglePanel.onClick.AddListener(TogglePanel);
        }

        public void Populate(AnimalDataSO animal)
        {
            if (animal == null) return;
            currentAnimal = animal;

            if (txtCommonName  != null) txtCommonName.text = animal.commonName;
            if (txtLatinName   != null) txtLatinName.text  = animal.latinName;
            if (txtStatusBadge != null) txtStatusBadge.text = GetStatusText(animal.iucnStatus);
            if (imgStatusBadge != null) imgStatusBadge.color = GetStatusColor(animal.iucnStatus);

            ShowTab(0);
            if (panelContent != null) panelContent.SetActive(true);
            isPanelExpanded = true;
        }

        private void ShowTab(int index)
        {
            currentTab = index;
            if (txtContent == null || currentAnimal == null) return;

            if      (index == 0) txtContent.text = BuildDeskripsi();
            else if (index == 1) txtContent.text = BuildHabitat();
            else if (index == 2) txtContent.text = BuildMitigasi();
            else if (index == 3) txtContent.text = BuildFakta();

            UpdateTabColors(index);
        }

        private string BuildDeskripsi()
        {
            string s = "";
            if (!string.IsNullOrEmpty(currentAnimal.description))
                s += currentAnimal.description + "\n\n";
            if (!string.IsNullOrEmpty(currentAnimal.diet))
                s += "<b>Pola Makan:</b> " + currentAnimal.diet + "\n\n";
            if (!string.IsNullOrEmpty(currentAnimal.daerahAsal))
                s += "<b>Daerah Asal:</b> " + currentAnimal.daerahAsal;
            return s.TrimEnd();
        }

        private string BuildHabitat()
        {
            return string.IsNullOrEmpty(currentAnimal.habitat)
                ? "Data habitat belum tersedia."
                : currentAnimal.habitat;
        }

        private string BuildMitigasi()
        {
            string s = "";
            if (!string.IsNullOrEmpty(currentAnimal.tingkatBahaya))
                s += "<b>Tingkat Bahaya:</b> " + currentAnimal.tingkatBahaya + "\n\n";
            if (!string.IsNullOrEmpty(currentAnimal.tindakanSaatBertemu))
                s += "<b>Tindakan Saat Bertemu:</b>\n" + currentAnimal.tindakanSaatBertemu;
            return string.IsNullOrEmpty(s) ? "Data mitigasi belum tersedia." : s.TrimEnd();
        }

        private string BuildFakta()
        {
            return string.IsNullOrEmpty(currentAnimal.funFact)
                ? "Fakta unik belum tersedia."
                : currentAnimal.funFact;
        }

        private void UpdateTabColors(int active)
        {
            Image[] tabs = { imgTabDeskripsi, imgTabHabitat, imgTabMitigasi, imgTabFakta };
            for (int i = 0; i < tabs.Length; i++)
                if (tabs[i] != null)
                    tabs[i].color = (i == active) ? TabActiveColor : TabInactiveColor;
        }

        private void PlayAudio()
        {
            if (currentAnimal == null || audioSource == null) return;
            if (currentAnimal.animalSound != null)
            {
                audioSource.clip = currentAnimal.animalSound;
                audioSource.Play();
            }
        }

        private void TogglePanel()
        {
            isPanelExpanded = !isPanelExpanded;
            if (panelContent != null) panelContent.SetActive(isPanelExpanded);
        }

        private string GetStatusText(ConservationStatus status)
        {
            if (status == ConservationStatus.CriticallyEndangered) return "Kritis (CR)";
            if (status == ConservationStatus.Endangered)           return "Terancam (EN)";
            if (status == ConservationStatus.Vulnerable)           return "Rentan (VU)";
            return "Risiko Rendah (LC)";
        }

        private Color GetStatusColor(ConservationStatus status)
        {
            if (status == ConservationStatus.CriticallyEndangered) return ColorCR;
            if (status == ConservationStatus.Endangered)           return ColorEN;
            if (status == ConservationStatus.Vulnerable)           return ColorVU;
            return ColorLC;
        }
    }
}
