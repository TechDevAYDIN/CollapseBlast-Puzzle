using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TDA.BlastTest;

namespace TDA.BlastTest
{
    public class GameField : MonoBehaviour
    {
        public static GameField Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType(typeof(GameField)) as GameField;

                return instance;
            }
            set
            {
                instance = value;
            }
        }
        private static GameField instance;


        public Level level;
        public Vector2Int colRows;
        public Vector2Int size;
        public Transform fieldPivot;
        public GameObject squarePrefab;

        public Item[] items;
        public FieldSquare[] squares;
        [SerializeField] public Rect worldRect;

        

        private Vector2 sqSize;
        public GameObject[] blockColumn;
        public float[] speedColumns;
        //public float[] fallColumns;

        public int targetScore;

        readonly Vector2Int[] around = { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(0, 1) };
        // Start is called before the first frame update
        void Start()
        {
            if (Instance == null)
                Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            GameField.Instance.CheckMatches();
        }
        public void LoadLevel(Level level)
        {
            //targetScore = level.targetScore;
            this.level = level;
            colRows = level.colRows;
            size = colRows;
            blockColumn = new GameObject[colRows.x];
            speedColumns = new float[size.x];

            GenerateField();
            transform.position = transform.position + (fieldPivot.transform.position - (Vector3)GetCenter());
            GenItems(false);
            //PreCreateItems();
            
            CorrectMatches(items.ToList());
            StartCoroutine(CheckNoMatch());
        }
        public Vector2 GetCenter()
        {
            worldRect = GetWorldRect();
            return worldRect.center;
        }

        private Rect GetWorldRect()
        {
            var minX = squares.Min(x => x.GetWorldPosition().x);
            var minY = squares.Min(x => x.GetWorldPosition().y);
            var maxX = squares.Max(x => x.GetWorldPosition().x);
            var maxY = squares.Max(x => x.GetWorldPosition().y);
            return Rect.MinMaxRect(minX - sqSize.x / 2, minY - sqSize.y / 2, maxX + sqSize.x / 2, maxY + sqSize.y / 2);
        }
        private void GenerateField()
        {
            squares = new FieldSquare[size.x * size.y];
            items = new Item[size.x * size.y];

            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    CreateSquare(x, y);
                }
            }
        }
        private void CreateSquare(int x, int y)
        {
            var square = Instantiate(squarePrefab.GetComponent<FieldSquare>(), transform);
            square.name = "Square_" + x + "_" + y;
            square.transform.localPosition = SetSquarePosition(x, y, square.size);
            square.pos = new Vector2Int(x, y);
            sqSize = square.size;
            squares[y * size.x + x] = square;

            square.AdjustOffset();
        }
        Vector3 SetSquarePosition(int x, int y, Vector2 squareSize)
        {
            var halfX = size.x / 2f;
            var halfY = size.y / 2f;
            var x1 = (x - halfX);
            var y1 = (-y + halfY);
            var squarePosition = new Vector3(x1 * squareSize.x, y1 * squareSize.y);
            return squarePosition;
        }
        public List<Item> GenItems(bool fall = true)
        {
            List<Item> newItems = new List<Item>();
            for (int x = 0; x < size.x; x++)
            {
                if (blockColumn[x] && blockColumn[x].activeSelf) continue;
                int verticalIndex = 0;
                for (int y = GetBottomEmptySquare(x); y >= 0; y--)
                {
                    var index = y * size.x + x;
                    var indexBelow = y + 1 * size.x + x;
                    if (items[index] == null)
                    {
                        var item = CreateItem("Item", index, new Vector2Int(x, y), fall, verticalIndex);
                        newItems.Add(item);
                        item.SetRandomColor();
                        verticalIndex++;
                    }
                }
            }
            return newItems;
        }
        public void FallAndGenerateItems()
        {
            PrepareFallItems();
            var list = GenItems();

        }
        public void PrepareFallItems()
        {
            for (var index = 0; index < speedColumns.Length; index++) speedColumns[index] = Random.Range(LevelManager.Instance.fallSpeed, LevelManager.Instance.fallSpeed + 5f);

            for (int x = 0; x < size.x; x++)
            {
                for (int y = size.y - 1; y >= 0; y--)
                {
                    if (blockColumn[x] && blockColumn[x].activeSelf) continue;
                    var item = items[y * size.x + x];
                    if (item != null && item.pos.x == x && item.pos.y == y)
                    {
                        if (!item.falling) item.CheckBelow(x, y);
                        else break;
                    }
                }
            }
        }
        public void CorrectMatches(List<Item> newItems)
        {
            CheckMatches();
        }
        int GetBottomEmptySquare(int x)
        {
            for (int y = 0; y < size.y; y++)
            {
                var index = y * size.x + x;
                if (items[index] != null && !items[index].canFallThrough) return y - 1;
            }
            return size.y - 1;
        }
        private Item CreateItem(string name, int index, Vector2Int pos, bool fall, int verticalIndex)
        {
            FieldSquare square = squares[index];
            var pooledObject = GetPooledObject(name);

            var item = pooledObject.GetComponent<Item>();
            FillItemsArray(item, pos.x, pos.y);
            var targetPosition = square.GetWorldPosition();
            item.transform.position = targetPosition;
            item.index = index;
            item.pos = pos;
            if (fall)
            {
                var position = item.transform.position;
                position = new Vector2(position.x, worldRect.yMax * 1.08f + verticalIndex * square.size.y);
                item.transform.position = position;
                item.MoveTo();
            }
            return item;
        }
        public void CheckMatches(int matchAmount = 2)
        {
            LevelManager.Instance.matches.Clear();
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    var item = items[y * size.x + x];
                    if (item != null)
                    {
                        var cMCount = item.CheckMatches();
                        if (cMCount >= matchAmount)
                        {
                            LevelManager.Instance.AddMatches();                            
                        }
                        if (cMCount <= LevelManager.Instance.level.iconPhase1) 
                            item.SetIcon(0);

                        else if (cMCount <= LevelManager.Instance.level.iconPhase2) 
                            item.SetIcon(1);
                        else if (cMCount <= LevelManager.Instance.level.iconPhase3)
                            item.SetIcon(2);
                        else if (cMCount > LevelManager.Instance.level.iconPhase3)
                            item.SetIcon(3);
                        else
                            item.SetIcon(0);
                    }
                }
            }
        }
        public Item[] GetItemsCross(Item item)
        {
            Item[] list = new Item[4];
            for (int i = 0; i < list.Length; i++)
            {
                list[i] = null;
            }
            var l = GetCrossVectors(item.pos);
            for (var i = 0; i < l.Length; i++)
            {
                var vector2Int = l[i];
                if (vector2Int == Vector2Int.left * 1000) continue;
                var nextItemIndex = vector2Int.y * Instance.size.x + vector2Int.x;
                var item1 = Instance.items[nextItemIndex];
                if (item1 != null) list[i] = item1;
            }
            return list;
        }
        public Vector2Int[] GetCrossVectors(Vector2Int pos)
        {
            Vector2Int[] v = new Vector2Int[4];
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = Vector2Int.left * 1000;
            }
            for (int i = 0; i < around.Length; i++)
            {
                Vector2Int newPos = pos + around[i];
                if (!IsInsideField(newPos)) continue;
                v[i] = newPos;
            }
            return v;
        }
        public bool IsInsideField(Vector2Int newPos)
        {
            return newPos.x >= 0 && newPos.x < Instance.size.x && newPos.y >= 0 && newPos.y < Instance.size.y;
        }
        private GameObject GetPooledObject(string name)
        {
            GameObject pooledObject = ObjectPooler.SharedInstance.GetPooledObject(name);
            if (pooledObject == null)
            {
                Debug.LogError(name + " object not found in pool");
                return pooledObject;
            }
            return pooledObject;
        }
        public void FillItemsArray(Item item, int x, int y, bool destroyItems = true)
        {
            var expandedPos = new Vector2Int(x, y);
            if (expandedPos.x < size.x && expandedPos.y < size.y)
            {
                var index0 = expandedPos.y * size.x + expandedPos.x;
                var itemToDelete = items[index0];
                if (itemToDelete != null && destroyItems) ObjectPooler.SharedInstance.PutBack(itemToDelete.gameObject);
                items[index0] = item;
            }
        }
        IEnumerator CheckNoMatch()
        {
            while (true)
            {
                if (NoMatchCondition())
                {

                    for (int i = 0; i < 5; i++)
                    {
                        yield return new WaitForSeconds(0.1f);
                        if (!NoMatchCondition()) break;
                    }                    
                    if (NoMatchCondition())
                    {
                        StartCoroutine(LevelManager.Instance.RegenLevel());
                        yield return new WaitForSeconds(10);
                    }
                }
                yield return new WaitForSeconds(0.5f);
            }

            bool NoMatchCondition()
            {
                return LevelManager.Instance.matches.Count == 0 && GameField.Instance.items != null;
            }
        }
    }
}