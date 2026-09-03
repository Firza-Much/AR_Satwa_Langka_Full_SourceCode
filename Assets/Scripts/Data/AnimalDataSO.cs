using UnityEngine;

namespace SatwaLangka.Data
{
    public enum ConservationStatus
    {
        CriticallyEndangered, // Kritis
        Endangered,           // Terancam
        Vulnerable,           // Rentan
        LeastConcern          // Risiko Rendah
    }

    public enum SatwaCategory
    {
        KulitTebal,
        BercangkangDanBersisik,
        MamaliaBerbuluPendek
    }

    [CreateAssetMenu(fileName = "NewAnimalData", menuName = "Satwa Langka/Animal Data", order = 1)]
    public class AnimalDataSO : ScriptableObject
    {
        [Header("Identifikasi")]
        public string animalCode;          // e.g. SATWA01
        public string commonName;          // e.g. Gajah Sumatra
        public string latinName;           // e.g. Elephas maximus sumatranus
        public SatwaCategory category;
        public ConservationStatus iucnStatus;

        [Header("Informasi Edukasi & Konservasi (BRIN/KLHK)")]
        public string daerahAsal;          // e.g. "Sumatera", "Ujung Kulon, Banten"
        public string tingkatBahaya;       // e.g. "Sangat Berbahaya", "Tidak Berbahaya"
        [TextArea(2, 5)]
        public string tindakanSaatBertemu; // Panduan mitigasi keselamatan saat bertemu di alam liar
        [TextArea(3, 6)]
        public string description;
        [TextArea(2, 4)]
        public string habitat;
        [TextArea(2, 4)]
        public string diet;
        [TextArea(2, 4)]
        public string funFact;

        [Header("Aset Visual & Audio")]
        public GameObject modelPrefab;
        public AudioClip animalSound;
        public Sprite thumbnail;
        
        [Header("Parameter AR")]
        public Vector3 defaultScale = Vector3.one * 0.2f;
        public float rotationSpeed = 200f;
    }
}
