using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SatwaLangka.Data;

namespace SatwaLangka.UI
{
    public class ModernARHUD : MonoBehaviour
    {
        [Header("Header HUD")]
        [SerializeField] private TextMeshProUGUI txtTitle;
        [SerializeField] private Button btnBack;
        [SerializeField] private Button btnScanReset;

        [Header("Scan Reticle")]
        [SerializeField] private RectTransform scanReticle;
        [SerializeField] private TextMeshProUGUI txtScanGuide;

        [Header("Bottom Sheet Card")]
        [SerializeField] private RectTransform bottomSheetCard;
        [SerializeField] private TextMeshProUGUI txtCommonName;
        [SerializeField] private TextMeshProUGUI txtLatinName;
        [SerializeField] private TextMeshProUGUI txtIucnBadge;
        [SerializeField] private Image imgIucnBadgeBg;
        [SerializeField] private TextMeshProUGUI txtContentBody;
        [SerializeField] private Button btnAudioPlay;
        [SerializeField] private Button btnToggleDetails;

        [Header("Tabs")]
        [SerializeField] private Button btnTabDeskripsi;
        [SerializeField] private Button btnTabHabitat;
        [SerializeField] private Button btnTabFaktaUnik;

        [Header("Data Satwa Saat Ini")]
        [SerializeField] private AnimalDataSO currentAnimal;

        private AudioSource audioSource;
        private bool isDetailsExpanded = false;
        private enum TabIndex { Deskripsi, Habitat, FaktaUnik }
        private TabIndex activeTab = TabIndex.Deskripsi;

        void Awake()
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            if (btnAudioPlay != null) btnAudioPlay.onClick.AddListener(PlayAnimalSound);
            if (btnTabDeskripsi != null) btnTabDeskripsi.onClick.AddListener(() => SwitchTab(TabIndex.Deskripsi));
            if (btnTabHabitat != null) btnTabHabitat.onClick.AddListener(() => SwitchTab(TabIndex.Habitat));
            if (btnTabFaktaUnik != null) btnTabFaktaUnik.onClick.AddListener(() => SwitchTab(TabIndex.FaktaUnik));
            if (btnToggleDetails != null) btnToggleDetails.onClick.AddListener(ToggleDetails);
        }

        void Start()
        {
            StartReticlePulse();
            if (currentAnimal != null)
            {
                DisplayAnimalData(currentAnimal);
            }
        }

        private void StartReticlePulse()
        {
            if (scanReticle != null)
            {
                // Pulsing scale animation
                LeanTween.scale(scanReticle, new Vector3(1.08f, 1.08f, 1f), 1.2f)
                    .setEaseInOutSine()
                    .setLoopPingPong();
            }
        }

        public void DisplayAnimalData(AnimalDataSO data)
        {
            currentAnimal = data;
            if (data == null) return;

            if (txtCommonName != null) txtCommonName.text = data.commonName;
            if (txtLatinName != null) txtLatinName.text = "<i>" + data.latinName + "</i>";

            // Status Badge
            if (txtIucnBadge != null)
            {
                switch (data.iucnStatus)
                {
                    case ConservationStatus.CriticallyEndangered:
                        txtIucnBadge.text = "KRITIS (CR)";
                        if (imgIucnBadgeBg != null) imgIucnBadgeBg.color = new Color(0.9f, 0.2f, 0.2f, 0.9f);
                        break;
                    case ConservationStatus.Endangered:
                        txtIucnBadge.text = "TERANCAM (EN)";
                        if (imgIucnBadgeBg != null) imgIucnBadgeBg.color = new Color(0.95f, 0.5f, 0.1f, 0.9f);
                        break;
                    case ConservationStatus.Vulnerable:
                        txtIucnBadge.text = "RENTAN (VU)";
                        if (imgIucnBadgeBg != null) imgIucnBadgeBg.color = new Color(0.95f, 0.75f, 0.1f, 0.9f);
                        break;
                    case ConservationStatus.LeastConcern:
                        txtIucnBadge.text = "RISIKO RENDAH (LC)";
                        if (imgIucnBadgeBg != null) imgIucnBadgeBg.color = new Color(0.1f, 0.75f, 0.4f, 0.9f);
                        break;
                }
            }

            SwitchTab(TabIndex.Deskripsi);

            // Animate Bottom Sheet slide in with Spring Easing
            if (bottomSheetCard != null)
            {
                bottomSheetCard.anchoredPosition = new Vector2(0, -500f);
                LeanTween.move(bottomSheetCard, new Vector3(0, 40f, 0), 0.6f)
                    .setEaseOutBack();
            }
        }

        private void SwitchTab(TabIndex tab)
        {
            activeTab = tab;
            if (currentAnimal == null || txtContentBody == null) return;

            switch (tab)
            {
                case TabIndex.Deskripsi:
                    txtContentBody.text = currentAnimal.description;
                    break;
                case TabIndex.Habitat:
                    txtContentBody.text = "<b>Habitat:</b>\n" + currentAnimal.habitat + "\n\n<b>Makanan:</b>\n" + currentAnimal.diet;
                    break;
                case TabIndex.FaktaUnik:
                    txtContentBody.text = "✨ <b>Fakta Unik:</b>\n" + currentAnimal.funFact;
                    break;
            }

            // Subtle punch animation on text change
            LeanTween.scale(txtContentBody.gameObject, new Vector3(1.03f, 1.03f, 1f), 0.15f)
                .setEaseOutQuad()
                .setOnComplete(() => {
                    LeanTween.scale(txtContentBody.gameObject, Vector3.one, 0.15f);
                });
        }

        private void ToggleDetails()
        {
            isDetailsExpanded = !isDetailsExpanded;
            float targetY = isDetailsExpanded ? 300f : 40f;
            if (bottomSheetCard != null)
            {
                LeanTween.move(bottomSheetCard, new Vector3(0, targetY, 0), 0.4f).setEaseOutCubic();
            }
        }

        public void PlayAnimalSound()
        {
            if (currentAnimal != null && currentAnimal.animalSound != null)
            {
                audioSource.PlayOneShot(currentAnimal.animalSound);
                // Animate audio button punch
                if (btnAudioPlay != null)
                {
                    LeanTween.scale(btnAudioPlay.gameObject, new Vector3(1.2f, 1.2f, 1f), 0.15f)
                        .setEaseOutBack()
                        .setOnComplete(() => LeanTween.scale(btnAudioPlay.gameObject, Vector3.one, 0.15f));
                }
            }
        }
    }
}
