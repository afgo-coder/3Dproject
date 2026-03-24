using System;
using MiniMart.Core;
using MiniMart.Data;
using MiniMart.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniMart.Managers
{
    [Serializable]
    public class StoreExpansionStep
    {
        public string label = "확장";
        public int cost = 10000;
        public GameObject sectionRoot;
        public GameObject blockerToDisable;
    }

    public class StoreExpansionManager : MonoBehaviour
    {
        private static readonly int[] RecommendedExpansionCosts = { 15000, 28000, 45000, 70000 };

        [SerializeField] private int startingExpansionLevel = 0;
        [SerializeField] private StoreExpansionStep[] expansionSteps;

        private int _currentExpansionIndex;

        public bool HasRemainingExpansion => expansionSteps != null && _currentExpansionIndex < expansionSteps.Length;
        public int CurrentExpansionLevel => _currentExpansionIndex;

        private void Start()
        {
            ApplyRecommendedBalance();
            ApplyExpansionLevel(startingExpansionLevel);
        }

        public void ApplyExpansionLevel(int expansionLevel)
        {
            _currentExpansionIndex = Mathf.Clamp(expansionLevel, 0, expansionSteps != null ? expansionSteps.Length : 0);

            if (expansionSteps == null)
            {
                return;
            }

            for (int i = 0; i < expansionSteps.Length; i++)
            {
                if (expansionSteps[i].sectionRoot != null)
                {
                    expansionSteps[i].sectionRoot.SetActive(i < _currentExpansionIndex);
                }

                if (expansionSteps[i].blockerToDisable != null)
                {
                    expansionSteps[i].blockerToDisable.SetActive(i >= _currentExpansionIndex);
                }
            }
        }

        public bool TryBuyNextExpansion()
        {
            if (!HasRemainingExpansion)
            {
                UIFeedback.ShowStatus("이미 모든 매장 확장을 완료했습니다.");
                return false;
            }

            StoreExpansionStep step = expansionSteps[_currentExpansionIndex];
            if (!EconomyManager.Instance || !EconomyManager.Instance.TrySpend(step.cost))
            {
                UIFeedback.ShowStatus($"확장 실패: {step.cost:N0}원이 필요합니다.");
                return false;
            }

            if (step.sectionRoot != null)
            {
                step.sectionRoot.SetActive(true);
            }

            if (step.blockerToDisable != null)
            {
                step.blockerToDisable.SetActive(false);
            }

            AudioManager.Instance?.PlayExpansion();
            UIFeedback.ShowStatus($"매장 확장 완료: {step.label}");
            _currentExpansionIndex++;
            SaveManager.Instance?.SaveGame();
            return true;
        }

        public string GetNextExpansionLabel()
        {
            return HasRemainingExpansion ? expansionSteps[_currentExpansionIndex].label : "확장 없음";
        }

        public int GetNextExpansionCost()
        {
            return HasRemainingExpansion ? expansionSteps[_currentExpansionIndex].cost : 0;
        }

        private void ApplyRecommendedBalance()
        {
            if (expansionSteps == null)
            {
                return;
            }

            int count = Mathf.Min(expansionSteps.Length, RecommendedExpansionCosts.Length);
            for (int i = 0; i < count; i++)
            {
                if (expansionSteps[i] != null)
                {
                    expansionSteps[i].cost = RecommendedExpansionCosts[i];
                }
            }
        }
    }

    public enum StoreTimeSegment
    {
        Morning,
        Lunch,
        Evening,
        Night
    }

    public class StoreProgressionManager : MonoBehaviour
    {
        private static readonly int[] LevelScoreThresholds = { 0, 25000, 75000, 150000, 260000 };

        public static StoreProgressionManager Instance { get; private set; }

        public int LifetimeSales { get; private set; }
        public int LifetimeCustomers { get; private set; }
        public int TotalGoalsCompleted { get; private set; }
        public int StoreLevel { get; private set; } = 1;
        public bool IsBulkOrderUnlocked => StoreLevel >= 2;
        public bool IsPromotionUnlocked => StoreLevel >= 3;
        public bool IsWorkerDiscountUnlocked => StoreLevel >= 4;
        public bool HasDemandForecastUnlocked => StoreLevel >= 5;
        public ProductCategory FeaturedCategory => GetFeaturedCategory();

        public event Action ProgressionChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureInstanceBeforeSceneLoad()
        {
            if (Instance != null)
            {
                return;
            }

            GameObject progressionObject = new GameObject("StoreProgressionManager");
            progressionObject.AddComponent<StoreProgressionManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            RecalculateLevel();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                Instance = null;
            }
        }

        public void RegisterSales(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            LifetimeSales += amount;
            RecalculateLevel();
        }

        public void RegisterCustomersServed(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            LifetimeCustomers += amount;
            RecalculateLevel();
        }

        public void RegisterGoalsCompleted(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            TotalGoalsCompleted += amount;
            RecalculateLevel();
        }

        public int GetAdditionalOrderAmount()
        {
            return IsBulkOrderUnlocked ? 2 : 0;
        }

        public int GetAdjustedWorkerCost(int baseCost)
        {
            float multiplier = IsWorkerDiscountUnlocked ? 0.85f : 1f;
            return Mathf.Max(1, Mathf.RoundToInt(baseCost * multiplier));
        }

        public int GetSalePrice(ProductData product)
        {
            if (product == null)
            {
                return 0;
            }

            int price = product.salePrice;
            if (IsPromotionUnlocked && product.category == FeaturedCategory)
            {
                price = Mathf.RoundToInt(price * 1.15f);
            }

            return Mathf.Max(0, price);
        }

        public StoreTimeSegment GetCurrentTimeSegment()
        {
            DayCycleManager dayCycleManager = FindFirstObjectByType<DayCycleManager>();
            if (dayCycleManager == null)
            {
                return StoreTimeSegment.Morning;
            }

            float normalizedTime = dayCycleManager.NormalizedTime;
            if (normalizedTime < 0.25f)
            {
                return StoreTimeSegment.Morning;
            }

            if (normalizedTime < 0.5f)
            {
                return StoreTimeSegment.Lunch;
            }

            if (normalizedTime < 0.8f)
            {
                return StoreTimeSegment.Evening;
            }

            return StoreTimeSegment.Night;
        }

        public float GetDemandMultiplier(ProductData product)
        {
            if (product == null)
            {
                return 1f;
            }

            float multiplier = 1f;
            switch (GetCurrentTimeSegment())
            {
                case StoreTimeSegment.Morning:
                    if (product.category == ProductCategory.Drink || product.category == ProductCategory.Snack)
                    {
                        multiplier += 0.3f;
                    }
                    break;
                case StoreTimeSegment.Lunch:
                    if (product.category == ProductCategory.InstantFood || product.category == ProductCategory.Meal)
                    {
                        multiplier += 0.45f;
                    }
                    break;
                case StoreTimeSegment.Evening:
                    if (product.category == ProductCategory.Meal || product.category == ProductCategory.DailyGoods)
                    {
                        multiplier += 0.35f;
                    }
                    break;
                case StoreTimeSegment.Night:
                    if (product.category == ProductCategory.Snack || product.category == ProductCategory.Drink)
                    {
                        multiplier += 0.25f;
                    }
                    break;
            }

            if (IsPromotionUnlocked && product.category == FeaturedCategory)
            {
                multiplier += 0.35f;
            }

            if (HasDemandForecastUnlocked)
            {
                multiplier += 0.1f;
            }

            return multiplier;
        }

        public string GetOperationsSummary()
        {
            string promotionLine = IsPromotionUnlocked
                ? $"오늘의 추천: {GetCategoryLabel(FeaturedCategory)}"
                : "오늘의 추천: 등급 3 해금";
            string bulkOrderLine = IsBulkOrderUnlocked
                ? $"대량 발주: 기본 +{GetAdditionalOrderAmount()}개"
                : "대량 발주: 등급 2 해금";
            string workerDiscountLine = IsWorkerDiscountUnlocked
                ? "분신 고용 할인: 적용 중"
                : "분신 할인: 등급 4 해금";

            return
                $"매장 등급: Lv.{StoreLevel}\n" +
                $"시간대: {GetTimeSegmentLabel(GetCurrentTimeSegment())}\n" +
                $"{promotionLine}\n" +
                $"{bulkOrderLine}\n" +
                $"{workerDiscountLine}";
        }

        public void RestoreProgression(int lifetimeSales, int lifetimeCustomers, int totalGoalsCompleted)
        {
            LifetimeSales = Mathf.Max(0, lifetimeSales);
            LifetimeCustomers = Mathf.Max(0, lifetimeCustomers);
            TotalGoalsCompleted = Mathf.Max(0, totalGoalsCompleted);
            RecalculateLevel();
        }

        private void RecalculateLevel()
        {
            int score = LifetimeSales + (LifetimeCustomers * 400) + (TotalGoalsCompleted * 2500);
            int level = 1;
            for (int i = 0; i < LevelScoreThresholds.Length; i++)
            {
                if (score >= LevelScoreThresholds[i])
                {
                    level = i + 1;
                }
            }

            StoreLevel = Mathf.Clamp(level, 1, LevelScoreThresholds.Length);
            ProgressionChanged?.Invoke();
        }

        private ProductCategory GetFeaturedCategory()
        {
            int day = GameManager.Instance != null ? Mathf.Max(1, GameManager.Instance.CurrentDay) : 1;
            ProductCategory[] categories =
            {
                ProductCategory.Drink,
                ProductCategory.Snack,
                ProductCategory.InstantFood,
                ProductCategory.Meal,
                ProductCategory.DailyGoods
            };

            int index = (day - 1) % categories.Length;
            return categories[index];
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ProgressionChanged?.Invoke();
        }

        private static string GetCategoryLabel(ProductCategory category)
        {
            switch (category)
            {
                case ProductCategory.Drink:
                    return "음료";
                case ProductCategory.Snack:
                    return "과자";
                case ProductCategory.InstantFood:
                    return "간편식";
                case ProductCategory.Meal:
                    return "식사류";
                case ProductCategory.DailyGoods:
                    return "생활용품";
                default:
                    return category.ToString();
            }
        }

        private static string GetTimeSegmentLabel(StoreTimeSegment segment)
        {
            switch (segment)
            {
                case StoreTimeSegment.Morning:
                    return "오전";
                case StoreTimeSegment.Lunch:
                    return "점심";
                case StoreTimeSegment.Evening:
                    return "저녁";
                case StoreTimeSegment.Night:
                    return "야간";
                default:
                    return segment.ToString();
            }
        }
    }
}
