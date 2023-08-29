using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace TDA.BlastTest
{
    public class LevelManager : MonoBehaviour
    {
        #region Instance
        public static LevelManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType(typeof(LevelManager)) as LevelManager;

                return instance;
            }
            set
            {
                instance = value;
            }
        }
        private static LevelManager instance;
        #endregion

        public bool canTouch = true;
        public int score = 0;
        public int targetscore = 1000;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI targetScoreText;
        public TextMeshProUGUI movesText;
        public Level level;
        public int moves = 20;
        public int maxColors;
        public float fallSpeed = 20;
        public GameField field;
        //bool levelLoaded;
        //matches on the field list
        public List<Matches> matches = new List<Matches>();
        //temp list of matched items 
        public List<Item> matchedSquares = new List<Item>();

        [SerializeField] GameObject scorePrefab;
        [SerializeField] GameObject scorePanel;

        // Start is called before the first frame update
        void Start()
        {
            
            if(PlayerPrefs.GetInt("isPlayerSelectLevel", 0) > 0)
            {
                LoadLevel(PlayerPrefs.GetInt("isPlayerSelectLevel"));
                PlayerPrefs.SetInt("isPlayerSelectLevel", 0);
            }
            else
            {
                LoadLevel(PlayerPrefs.GetInt("CurrentLevel", 1));
            }
            
        }

        // Update is called once per frame
        void Update()
        {
            scoreText.text = score.ToString();
            movesText.text = moves.ToString();
            if(moves == 0)
            {
                canTouch = false;
                if (score >= targetscore) GameManager.Singleton.LevelPassed();
                else GameManager.Singleton.LevelFailed();
            }
            if(moves > 0)
            {
                canTouch = true;
                if (score >= targetscore) 
                {
                    canTouch = false;
                    GameManager.Singleton.LevelPassed();
                } 
            }
        }
        public void LoadLevel(int num, int overrideColor = 0, int overrideMoves = 0, int overrideTargetScore = 0)
        {
            level = Resources.Load<Level>("Levels/Level_" + num);
            moves = overrideMoves == 0 ? level.moveCount : overrideMoves;
            maxColors = overrideColor == 0 ? level.colorSize : overrideColor;
            targetscore = overrideTargetScore == 0 ? level.targetScore : overrideTargetScore;
            targetScoreText.text = targetscore.ToString();
            field = FindObjectOfType<GameField>();
            field.LoadLevel(level);
        }

        //Adding matches to array
        public void AddMatches()
        {
            var v = matches.SelectMany(i => i.items);
            if (!v.Any(i => matchedSquares.Contains(i)))
            {
                matches.Add(new Matches { items = matchedSquares.ToArray() });
            }
        }
        public void ActivateItem(Item itemMatching)
        {
            if(canTouch)
                StartCoroutine(ActivateItemAndMatch(itemMatching));
        }
        IEnumerator ActivateItemAndMatch(Item itemMatching)
        {
            Item[] matchedItems = { };
            float animTime = 0.1f;
            foreach (var matchese in matches)
            {
                if (matchese.items.Any(i => i == itemMatching)) matchedItems = matchese.items;
            }

            if (matchedItems.Length > 0)
            {
                ShowScorePop(matchedItems.Sum(i => i.score), itemMatching.transform.position);
                moves -= 1;
            }
            foreach (var item in matchedItems)
            {
                if (!item) continue;
                item.transform.DOMove(itemMatching.transform.position, animTime).OnComplete(() => { item.DestroyItemStart(); });
            }
            if (matchedItems.Length == 0) yield break;
            yield return new WaitForFixedUpdate();
            yield return new WaitWhile(() => GameField.Instance.items.Any(x => x?.falling ?? false));
            yield return new WaitForSeconds(0.25f);
            FinishDestroy();
        }
        public void ShowScorePop(int score, Vector3 transformPosition)
        {
            GameObject popScore = Instantiate(scorePrefab,transformPosition,Quaternion.identity,scorePanel.transform);
            popScore.GetComponent<TextMeshProUGUI>().text = score.ToString();
            popScore.SetActive(true);
            this.score += score;
        }
        public void FinishDestroy()
        {
            GameField.Instance.FallAndGenerateItems();
        }

        public IEnumerator RegenLevel()
        {
            yield return new WaitForSeconds(.5f);
            var list = GameField.Instance.items.Where(i => !(i is null) && i.CompareTag("Item"));
            foreach (var i in list)
            {
                yield return new WaitForSeconds(.05f);
                i.MixBoard();
            }
        }
    }
    [System.Serializable]
    public class Matches
    {
        public Item[] items;
    }
}