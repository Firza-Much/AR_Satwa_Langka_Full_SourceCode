using UnityEngine;

namespace SatwaLangka.UI
{
    /// <summary>
    /// Holds card prefab reference. Place in Resources/ so it's always available at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "CardPrefabConfig", menuName = "Satwa Langka/Card Prefab Config")]
    public class CardPrefabConfig : ScriptableObject
    {
        public GameObject cardPrefab;
    }
}
