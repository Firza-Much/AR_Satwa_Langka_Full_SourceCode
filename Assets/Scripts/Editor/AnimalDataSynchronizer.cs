using UnityEngine;
using UnityEditor;
using System.IO;
using SatwaLangka.Data;

namespace SatwaLangka.EditorScripts
{
    public static class AnimalDataSynchronizer
    {
        [MenuItem("Satwa Langka/Sync All 12 Animal Data (BRIN / KLHK)")]
        public static void SyncAll12()
        {
            string dataDir = "Assets/Data/Animals";
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

            var list = new[]
            {
                // ==========================================
                // 1. SATWA KULIT TEBAL
                // ==========================================
                new { 
                    Code = "SATWA01", 
                    Name = "Gajah Sumatra", 
                    Latin = "Elephas maximus sumatranus", 
                    Cat = SatwaCategory.KulitTebal, 
                    Status = ConservationStatus.CriticallyEndangered,
                    Daerah = "Sumatra",
                    Bahaya = "Cukup Berbahaya",
                    Mitigasi = "Jaga jarak aman minimal 20 meter. Jangan membuat gerakan tiba-tiba atau suara bising yang mengejutkan kawanan.",
                    Desc = "Gajah Sumatra adalah subspesies gajah asia yang hidup di pulau Sumatra. Berperan penting sebagai 'insinyur ekosistem' penyebar biji dan pembuka koridor alami di hutan hujan tropis.",
                    Hab = "Hutan hujan tropis dataran rendah hingga rawa gambut pulau Sumatra", 
                    Diet = "Rumput liar, daun muda, bambu, kulit kayu, dan buah hutan", 
                    Fact = "Memiliki ukuran tubuh yang lebih kompak dan telinga yang lebih kecil dibanding gajah afrika, serta berperan krusial dalam regenerasi vegetasi hutan.",
                    Folder = "Gajah_Sumatra" 
                },

                new { 
                    Code = "SATWA02", 
                    Name = "Banteng Jawa", 
                    Latin = "Bos javanicus", 
                    Cat = SatwaCategory.KulitTebal, 
                    Status = ConservationStatus.Endangered,
                    Daerah = "Jawa (TN Baluran, Ujung Kulon, Alas Purwo)",
                    Bahaya = "Sedang - Tinggi",
                    Mitigasi = "Jaga jarak aman dan jangan berdiri di antara induk dan anaknya. Cari pohon besar untuk berlindung jika banteng jantan merasa terancam.",
                    Desc = "Banteng Jawa adalah banteng liar asli Nusantara dengan ciri khas bercak putih pada kaki menyerupai kaos kaki dan pantat. Hidup berkelompok di kawasan konservasi hutan dan savana.",
                    Hab = "Hutan jati, hutan musim, dan savana terbuka kawasan konservasi pulau Jawa", 
                    Diet = "Rumput liar, semak belukar, daun bambu, dan tunas muda", 
                    Fact = "Banteng jantan dewasa memiliki mantel tubuh hitam pekat dengan tanduk melengkung kokoh, sedangkan betina dan anak berwarna cokelat keemasan.",
                    Folder = "Banteng_Jawa" 
                },

                new { 
                    Code = "SATWA03", 
                    Name = "Anoa", 
                    Latin = "Bubalus depressicornis", 
                    Cat = SatwaCategory.KulitTebal, 
                    Status = ConservationStatus.Endangered,
                    Daerah = "Sulawesi",
                    Bahaya = "Sedang - Tinggi",
                    Mitigasi = "Jauhkan diri perlahan, jangan membuat suara keras. Jangan pernah mendekat untuk berfoto karena anoa memiliki insting bertahan yang sangat agresif.",
                    Desc = "Anoa adalah spesies kerbau kerdil terkecil di dunia yang hanya ditemukan di pulau Sulawesi. Dikenal pemalu, hidup soliter di hutan primer pegunungan.",
                    Hab = "Hutan hujan tropis dataran rendah hingga pegunungan pulau Sulawesi", 
                    Diet = "Tumbuhan air, lumut, pakis, rumput, dan buah liar yang jatuh", 
                    Fact = "Tanduknya berbentuk segitiga lurus meruncing ke belakang yang berfungsi melindungi leher saat menerobos vegetasi semak lebat.",
                    Folder = "Anoa" 
                },

                new { 
                    Code = "SATWA04", 
                    Name = "Babirusa", 
                    Latin = "Babyrousa babyrussa", 
                    Cat = SatwaCategory.KulitTebal, 
                    Status = ConservationStatus.Vulnerable,
                    Daerah = "Sulawesi & Kepulauan Maluku",
                    Bahaya = "Rendah - Sedang",
                    Mitigasi = "Amati dari jarak jauh tanpa mengganggu jalur jalannya. Jangan mencoba menyentuh atau mengejar ke dalam semak.",
                    Desc = "Babirusa memiliki taring atas yang menembus moncong dan melengkung ke arah dahi. Hewan purba unik endemik kepulauan Wallacea Nusantara.",
                    Hab = "Rawa, tepian sungai, dan hutan hujan tropis pulau Sulawesi", 
                    Diet = "Buah hutan jatuh, umbi-umbian, jamur, dan dedaunan", 
                    Fact = "Taring atasnya yang melengkung terus tumbuh sepanjang hidup dan digunakan oleh jantan untuk memikat pasangan serta pertarungan ritual.",
                    Folder = "Babirusa" 
                },

                // ==========================================
                // 2. BERCANGKANG DAN BERSISIK
                // ==========================================
                new { 
                    Code = "SATWA05", 
                    Name = "Sanca Batik", 
                    Latin = "Malayopython reticulatus", 
                    Cat = SatwaCategory.BercangkangDanBersisik, 
                    Status = ConservationStatus.LeastConcern,
                    Daerah = "Seluruh Kepulauan Nusantara",
                    Bahaya = "Sangat Berbahaya",
                    Mitigasi = "Jangan pernah mencoba menangkap ular berukuran besar sendirian. Menjauh secara perlahan dan hubungi petugas penanganan satwa liar.",
                    Desc = "Ular terpanjang di dunia dengan pola sisik geometris indah seperti batik. Sangat piawai memanjat pohon, berenang di sungai, dan berburu secara nokturnal.",
                    Hab = "Hutan tropis, rawa-rawa, perkebunan, hingga aliran sungai di Indonesia", 
                    Diet = "Mamalia kecil hingga sedang (kancil, babi hutan), burung, dan reptil", 
                    Fact = "Memiliki sensor lubang panas (*heat pits*) di sepanjang bibirnya yang memungkinkan deteksi mangsa berdarah panas dalam kegelapan total.",
                    Folder = "Sanca_Anaconda" 
                },

                new { 
                    Code = "SATWA06", 
                    Name = "Kura-Kura Moncong Babi", 
                    Latin = "Carettochelys insculpta", 
                    Cat = SatwaCategory.BercangkangDanBersisik, 
                    Status = ConservationStatus.Endangered,
                    Daerah = "Papua Selatan (Sungai Lorentz, Asmat)",
                    Bahaya = "Tidak Berbahaya",
                    Mitigasi = "Jangan disentuh atau diangkat dari air. Dilarang memperjualbelikan atau mengambil telurnya di tepi sungai.",
                    Desc = "Kura-kura air tawar bertempurung lunak khas Papua dengan moncong berdaging seperti hidung babi dan tungkai berbentuk dayung sejati.",
                    Hab = "Sungai air tawar berarus tenang, danau, dan estuari Papua Selatan", 
                    Diet = "Buah ara (*Ficus*) yang jatuh, kepiting air tawar, moluska, dan alga", 
                    Fact = "Merupakan satu-satunya spesies kura-kura air tawar di dunia yang berevolusi memiliki kaki dayung menyerupai penyu laut sejati.",
                    Folder = "Kura_Kura_Rawa" 
                },

                new { 
                    Code = "SATWA07", 
                    Name = "Kura-Kura Leher Ular Rote", 
                    Latin = "Chelodina mccordi", 
                    Cat = SatwaCategory.BercangkangDanBersisik, 
                    Status = ConservationStatus.CriticallyEndangered,
                    Daerah = "Pulau Rote, Nusa Tenggara Timur",
                    Bahaya = "Tidak Berbahaya",
                    Mitigasi = "Lindungi habitat perairannya dari sampah dan pestisida. Segera laporkan ke balai konservasi jika menemukan di alam bebas.",
                    Desc = "Kura-kura sangat langka endemik Pulau Rote dengan leher panjang fleksibel menyerupai ular yang ditekuk ke samping saat bersembunyi di tempurung.",
                    Hab = "Danau dangkal, rawa air tawar, dan sawah tergenang Pulau Rote (NTT)", 
                    Diet = "Ikan kecil, udang air tawar, berudu, jentik-jentik, dan serangga air", 
                    Fact = "Lehernya yang sangat panjang tidak dapat ditarik lurus ke dalam tempurung melainkan dilipat menyamping (*pleurodira*).",
                    Folder = "Kura_Kura_Rawa" 
                },

                new { 
                    Code = "SATWA08", 
                    Name = "Trenggiling", 
                    Latin = "Manis javanica", 
                    Cat = SatwaCategory.BercangkangDanBersisik, 
                    Status = ConservationStatus.CriticallyEndangered,
                    Daerah = "Sumatra, Jawa, Kalimantan",
                    Bahaya = "Tidak Berbahaya",
                    Mitigasi = "Jangan disentuh atau diangkat. Biarkan ia lewat dengan bebas. Mekanisme pertahanan alaminya hanya menggulung tubuh menjadi bola.",
                    Desc = "Mamalia pemakan semut dan rayap yang tubuhnya dilindungi sisik keratin tebal berlapis. Berperan penting sebagai pengendali alami hama hutan.",
                    Hab = "Hutan hujan tropis primer, sekunder, dan perkebunan di Jawa, Sumatra, Kalimantan", 
                    Diet = "Semut tanah dan rayap pohon", 
                    Fact = "Panjang lidahnya saat menjulur melebihi panjang tubuhnya sendiri, dan tidak memiliki gigi melainkan menggiling makanan di perutnya.",
                    Folder = "Trenggiling" 
                },

                // ==========================================
                // 3. MAMALIA BERBULU PENDEK
                // ==========================================
                new { 
                    Code = "SATWA09", 
                    Name = "Macan Tutul Jawa", 
                    Latin = "Panthera pardus melas", 
                    Cat = SatwaCategory.MamaliaBerbuluPendek, 
                    Status = ConservationStatus.CriticallyEndangered,
                    Daerah = "Pulau Jawa",
                    Bahaya = "Sangat Berbahaya",
                    Mitigasi = "Jangan panik atau lari membelakangi. Tatap matanya, perlahan mundur tanpa berbalik badan, dan buat postur tubuh terlihat lebih besar.",
                    Desc = "Kucing besar predator puncak terakhir yang bertahan di pulau Jawa. Memiliki varian mantel bertotol emas roset dan varian hitam pekat (Macan Kumbang).",
                    Hab = "Hutan pegunungan lebat, hutan lindung, dan lereng gunung berapi di pulau Jawa", 
                    Diet = "Rusa timor, babi hutan, kancil, monyet ekor panjang, dan ungko", 
                    Fact = "Pemanjat pohon ulung yang sering membawa dan menyembunyikan hasil buruannya di atas dahan pohon tinggi untuk menghindari predator lain.",
                    Folder = "Macan_Tutul" 
                },

                new { 
                    Code = "SATWA10", 
                    Name = "Rusa Timor", 
                    Latin = "Rusa timorensis", 
                    Cat = SatwaCategory.MamaliaBerbuluPendek, 
                    Status = ConservationStatus.Vulnerable,
                    Daerah = "Jawa, Bali, Nusa Tenggara (Kepulauan Sunda Kecil)",
                    Bahaya = "Rendah",
                    Mitigasi = "Amati dari kejauhan. Hindari mendekati pejantan dewasa selama musim kawin karena tanduknya bisa digunakan untuk menanduk.",
                    Desc = "Rusa bertanduk gagah khas kepulauan Sunda Kecil dan Jawa. Sangat tangguh beradaptasi di lingkungan savana terik dengan indera pendengaran tajam.",
                    Hab = "Padang savana terbuka, padang rumput pesisir, dan hutan gugur tropis", 
                    Diet = "Rumput savana, daun muda semak, pucuk pohon, dan herba liar", 
                    Fact = "Sangat mahir berenang menyeberangi selat antar-pulau kecil di kawasan Indonesia timur untuk mencari padang rumput baru.",
                    Folder = "Rusa_Jawa" 
                },

                new { 
                    Code = "SATWA11", 
                    Name = "Sigung", 
                    Latin = "Mydaus javanensis", 
                    Cat = SatwaCategory.MamaliaBerbuluPendek, 
                    Status = ConservationStatus.LeastConcern,
                    Daerah = "Jawa, Madura, Sumatra, Kalimantan",
                    Bahaya = "Rendah (Aroma Menyengat)",
                    Mitigasi = "Jangan mengejutkan atau memojokkannya. Sigung akan menyemprotkan cairan pertahanan berbau sangat menyengat jika merasa terancam.",
                    Desc = "Sigung Jawa (Teledu) memiliki tubuh kekar berbulu hitam lebat dengan garis putih memanjang di punggung. Dikenal memiliki aroma pertahanan khas.",
                    Hab = "Hutan pegunungan tinggi dan kawasan perbukitan sejuk di Jawa dan Sumatra", 
                    Diet = "Cacing tanah, kumbang, larva serangga, akar-akaran, dan buah liar", 
                    Fact = "Memiliki cakar depan yang sangat kokoh untuk menggali lapisan tanah humus demi mencari mangsa serangga bawah tanah.",
                    Folder = "Sigung" 
                },

                new { 
                    Code = "SATWA12", 
                    Name = "Bekantan", 
                    Latin = "Nasalis larvatus", 
                    Cat = SatwaCategory.MamaliaBerbuluPendek, 
                    Status = ConservationStatus.Endangered,
                    Daerah = "Kalimantan (Hutan Mangrove & Tepi Sungai)",
                    Bahaya = "Rendah",
                    Mitigasi = "Amati dari jarak jauh dengan tenang. Jangan membuat suara bising karena satwa ini sangat mudah mengalami stres di habitat alaminya.",
                    Desc = "Monyet berhidung panjang dan perut buncit khas hutan mangrove pulau Kalimantan (maskot fauna Kalimantan Selatan).",
                    Hab = "Hutan bakau (mangrove), rawa gambut, dan hutan riparian tepi sungai Kalimantan", 
                    Diet = "Pucuk daun bakau muda, buah mentah, biji-bijian, dan bunga hutan", 
                    Fact = "Memiliki selaput renang di antara jari-jari kakinya yang membuatnya sangat lihai berenang menyeberang sungai beraliran deras.",
                    Folder = "Bekantan" 
                }
            };

            for (int i = 0; i < list.Length; i++)
            {
                var item = list[i];
                string cleanName = item.Name.Replace(" ", "_").Replace("/", "_");
                string assetPath = $"{dataDir}/{item.Code}_{cleanName}.asset";

                AnimalDataSO so = AssetDatabase.LoadAssetAtPath<AnimalDataSO>(assetPath);
                if (so == null)
                {
                    string[] existing = AssetDatabase.FindAssets($"{item.Code}", new[] { dataDir });
                    if (existing.Length > 0)
                    {
                        string oldPath = AssetDatabase.GUIDToAssetPath(existing[0]);
                        so = AssetDatabase.LoadAssetAtPath<AnimalDataSO>(oldPath);
                        if (oldPath != assetPath)
                        {
                            AssetDatabase.RenameAsset(oldPath, $"{item.Code}_{cleanName}");
                        }
                    }
                }

                if (so == null)
                {
                    so = ScriptableObject.CreateInstance<AnimalDataSO>();
                    AssetDatabase.CreateAsset(so, assetPath);
                }

                so.animalCode = item.Code;
                so.commonName = item.Name;
                so.latinName = item.Latin;
                so.category = item.Cat;
                so.iucnStatus = item.Status;
                so.daerahAsal = item.Daerah;
                so.tingkatBahaya = item.Bahaya;
                so.tindakanSaatBertemu = item.Mitigasi;
                so.description = item.Desc;
                so.habitat = item.Hab;
                so.diet = item.Diet;
                so.funFact = item.Fact;

                string thumbPath = $"Assets/Sprites/Animals/Thumb_{item.Code}.png";
                Sprite thumb = AssetDatabase.LoadAssetAtPath<Sprite>(thumbPath);
                if (thumb != null) so.thumbnail = thumb;

                string modelPath = $"Assets/Models/{item.Folder}/{item.Folder}.glb";
                GameObject modelObj = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                if (modelObj != null) so.modelPrefab = modelObj;

                EditorUtility.SetDirty(so);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<b>[SATWA AR]</b> All 12 Animal Data successfully synchronized with BRIN/KLHK academic data!");
        }
    }
}
