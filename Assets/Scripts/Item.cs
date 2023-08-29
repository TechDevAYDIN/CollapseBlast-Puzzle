using TDA.BlastTest;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Item : MonoBehaviour
{

    public int index;
    public Vector2Int pos;
    public int Color = 0;
    public int State;
    public int score = 10;
    public bool canFallThrough;
    public Vector2Int size = Vector2Int.one;
    public SpriteRenderer spriteRenderer;
    public List<SpriteIcon> sprites = new List<SpriteIcon>();
    private Vector2 targetPosition;
    private IEnumerator moveToCor;

    private float startTimeGlobal;

    public bool falling;
    private Animator animator;
    private int localMatches;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        SetRandomColor();
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void SetRandomColor()
    {
        Color = Random.Range(0, LevelManager.Instance.maxColors);
        ChangeColor(Color);
    }
    public void MixBoard()
    {
        animator.SetTrigger("ColorChange");
        new WaitForSeconds(.55f);
        SetRandomColor();
    }
    public void MoveTo()
    {
        var speedColumn = GameField.Instance.speedColumns[(int)pos.x];
        var newPosition = GameField.Instance.squares[index].GetWorldPosition();
        if (targetPosition != newPosition && moveToCor != null)
            StopCoroutine(moveToCor);
        targetPosition = newPosition;
        moveToCor = MoveToCor(targetPosition, speedColumn / 1.5f);
        StartCoroutine(moveToCor);
    }

    IEnumerator MoveToCor(Vector2 destPos, float speed)
    {
        if (startTimeGlobal == 0)
            startTimeGlobal = Time.time;
        var startTime = Time.time;
        var distCovered = (Time.time - startTime) * speed;

        var startPos = transform.position;
        var distance = Vector2.Distance(startPos, destPos);

        float fracJourney = 0;
        while (fracJourney < 1)
        {
            falling = true;
            distCovered = (Time.time - startTime) * speed;
            speed += (Time.time - startTimeGlobal) / speed;
            fracJourney = distCovered / distance;
            transform.position = Vector2.Lerp(startPos, destPos, fracJourney);
            yield return new WaitForEndOfFrame();
            if (fracJourney > 0.5 && fracJourney < 0.7) CheckBelow(pos.x, pos.y);
        }
        CheckBelow(pos.x, pos.y);
        StopFall();
    }
    
    public int CheckMatches()
    {
        LevelManager.Instance.matchedSquares.Clear();
        LevelManager.Instance.matchedSquares.Add(this);
        CheckMatches(Color);
        localMatches = LevelManager.Instance.matchedSquares.Count;
        return localMatches;
    }


    private void CheckMatches(int color)
    {
        var list = GameField.Instance.GetItemsCross(this);
        foreach (var item in list)
        {
            if (!item) continue;
            if (item.falling) continue;
            var nextItemColor = item.Color;
            if ((color == nextItemColor && !LevelManager.Instance.matchedSquares.Exists(x => x == item)))
            {
                LevelManager.Instance.matchedSquares.Add(item);
                item.CheckMatches(color);
            }
        }
    }
    private void OnMouseDown()
    {
        ActivateItem();
    }

    public virtual void ActivateItem()
    {
        LevelManager.Instance.ActivateItem(this);
    }
    public void CheckBelow(int itemX, int itemY)
    {
        int oldIndex = itemY * GameField.Instance.size.x + itemX;
        int indexEmpty = -1;
        int index2 = -1;
        int wideCounter = 0;
        int wideCounterlast = 0;
        Vector2Int newPos = Vector2Int.zero;
        for (int y = itemY + size.y; y < GameField.Instance.size.y; y++)
        {
            Item nextitem = null;
            wideCounter = 0;
            FieldSquare sq = null;
            for (int x = itemX; x < Mathf.Clamp(itemX + size.x, 0, GameField.Instance.size.x); x++)
            {
                index2 = y * GameField.Instance.size.x + x;
                nextitem = GameField.Instance.items[index2];
                sq = GameField.Instance.squares[index2];
                if (nextitem == null)
                {
                    if (x == itemX)
                    {
                        indexEmpty = index2;
                    }

                    wideCounter++;
                }
            }

            if (wideCounter == size.x)
            {
                wideCounterlast = wideCounter;
                newPos.x = itemX;
                newPos.y = y - (size.y - 1);
            }
            if (nextitem != null || (wideCounter < size.x && wideCounter > 0))
                break;
        }

        if (indexEmpty <= -1 || wideCounterlast < size.x) return;
        else
        {
            GameField.Instance.items[oldIndex] = null;
            GameField.Instance.items[indexEmpty] = this;
        }

        index = indexEmpty;
        pos = newPos;
        MoveTo();
    }
    public virtual void DestroyItemStart()
    {
        DestroyItemFinish();
    }
    public void DestroyItemFinish()
    {
        if (GameField.Instance.items.Contains(this))
            GameField.Instance.FillItemsArray(null, pos.x, pos.y);
        var particleEffect = ObjectPooler.SharedInstance.GetPooledParticle("Particle"+Color);
        particleEffect.gameObject.transform.position = this.transform.position;
    }
    private void StopFall()
    {
        startTimeGlobal = 0;
        falling = false;
        animator?.SetTrigger("bounce");
    }
    private void ChangeColor(int color)
    {
        this.Color = color;
        UpdateIcon();
    }

    public void SetIcon(int i)
    {
        State = i;
        UpdateIcon();
    }

    void UpdateIcon()
    {
        spriteRenderer.sprite = sprites[Color]._sprites[State];
    }
}

[System.Serializable]
public class SpriteIcon
{
    public Sprite[] _sprites;
}
