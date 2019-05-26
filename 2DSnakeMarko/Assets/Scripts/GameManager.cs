using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;



namespace SA
{


    public class GameManager : MonoBehaviour
    {
        public int maxHeight = 15;
        public int maxWidth = 17;

        int curScore ;
        int highScore;

        public Color color1;
        public Color color2;
        public Color playerColor = Color.black;
        public Color appleColor = Color.red;

        public Transform CameraHoleder;

        GameObject appleObj;
        GameObject playerObj;
        GameObject tailParent;
        Node playerNode;
        Node appleNode;
        Sprite playerSprite;


        GameObject mapObject;
        SpriteRenderer mapRenderer;

        Node[,] grid;
        List<Node> availableNodes = new List<Node>();
        List<SpecialNode> tail = new List<SpecialNode>();

        bool up, left, right, down;
        public bool isGameOver;
        public bool isFirstInput;

        public float moveRate = 0.5f;
        float timer;

        Direction targetDirection;
        Direction currentDirection;

        public Text currentScoreText;
        public Text highScoreText;


        public enum Direction
        {
            up, down, left, right
        }

        public UnityEvent onStart;
        public UnityEvent onGameOver;
        public UnityEvent firstInput;
        public UnityEvent onScore;


        //-----------------INIT
        #region Init 

        private void Start()
        {
            onStart.Invoke();
        }

        public void StartNewGame()
        {
            ClearReferences();
            CreateMap();
            PlacePlayer();
            PlaceCamera();
            CreateApple();
            targetDirection = Direction.right;
            isGameOver = false;
            curScore = 0;
            DBManager.score = 0;
            UpdateScore();
        }

        public void ClearReferences()
        {
            if (mapObject != null)
                Destroy(mapObject);

            if (playerObj != null)
                Destroy(playerObj);

            if (appleObj != null)
                Destroy(appleObj);

            foreach (var t in tail)
            {
                if (t.obj != null)
                    Destroy(t.obj);
            }
            tail.Clear();
            availableNodes.Clear();
            grid = null;
        }


        void CreateMap()
        {
            mapObject = new GameObject("Map");
            mapRenderer = mapObject.AddComponent<SpriteRenderer>();

            grid = new Node[maxWidth, maxHeight];

            Texture2D txt = new Texture2D(maxWidth, maxHeight);
            for (int x = 0; x < maxWidth; x++)
            {
                for (int y = 0; y < maxHeight; y++)
                {
                    Vector3 tp = Vector3.zero;
                    tp.x = x;
                    tp.y = y;

                    Node n = new Node()
                    {
                        x = x,
                        y = y,
                        worldPosition = tp
                    };
                    grid[x, y] = n;
                    availableNodes.Add(n);

                    #region visual

                    if (x % 2 != 0)
                    {
                        if (y % 2 != 0)
                        {
                            txt.SetPixel(x, y, color1);
                        }
                        else
                        {
                            txt.SetPixel(x, y, color2);
                        }
                    }
                    else
                    {
                        if (y % 2 != 0)
                        {
                            txt.SetPixel(x, y, color2);
                        }
                        else
                        {
                            txt.SetPixel(x, y, color1);
                        }
                    }
                    #endregion
                }
            }
            txt.filterMode = FilterMode.Point;

            txt.Apply();
            Rect rect = new Rect(0, 0, maxWidth, maxHeight);
            Sprite sprite = Sprite.Create(txt, rect, Vector2.zero, 1, 0, SpriteMeshType.FullRect);
            mapRenderer.sprite = sprite;

        }

        void PlacePlayer()
        {
            playerObj = new GameObject("Player");
            SpriteRenderer playerRenderer = playerObj.AddComponent<SpriteRenderer>();
            playerSprite = CreateSprite(playerColor);
            playerRenderer.sprite = playerSprite;
            playerRenderer.sprite = CreateSprite(playerColor);
            playerRenderer.sortingOrder = 1;
            playerNode = GetNode(3, 3);

            PlacePlayerObject(playerObj, playerNode.worldPosition);
            playerObj.transform.localScale = Vector3.one * 0.95f;

            tailParent = new GameObject("tailParent");
        }

        void PlaceCamera()
        {
            Node n = GetNode(maxWidth / 2, maxHeight / 2);
            Vector3 p = n.worldPosition;
            p += Vector3.one * .5f;

            CameraHoleder.position = p;
        }

        void CreateApple()
        {
            appleObj = new GameObject("Apple");
            SpriteRenderer appleRenderer = appleObj.AddComponent<SpriteRenderer>();
            appleRenderer.sprite = CreateSprite(appleColor);
            appleRenderer.sortingOrder = 1;
            RandomlyPlaceApple();
        }

        #endregion
        //-----------------END INIT


        //-----------------UPDATE
        #region Update

        private void Update()
        {
            if (isGameOver)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    onStart.Invoke();
                }
                return;
            }
            GetInput();

            if (isFirstInput)
            {
                SetPlayerDirection();
                timer += Time.deltaTime;
                if (timer > moveRate)
                {
                    timer = 0;
                    currentDirection = targetDirection;
                    MovePlayer();
                }
            }
            else
            {
                if (up || down || left || right)
                {
                    isFirstInput = true;
                    firstInput.Invoke();
                }
            }
        }

        void GetInput()
        {
            up = Input.GetButtonDown("Up");
            down = Input.GetButtonDown("Down");
            left = Input.GetButtonDown("Left");
            right = Input.GetButtonDown("Right");
        }

        void SetPlayerDirection()
        {
            if (up)
            {
                SetDirection(Direction.up);

            }
            else if (down)
            {
                SetDirection(Direction.down);
            }
            else if (left)
            {
                SetDirection(Direction.left);
            }
            else if (right)
            {
                SetDirection(Direction.right);
            }

        }

        void SetDirection(Direction d)
        {
            if (!akoJeSupprotno(d))
            {
                targetDirection = d;
            }
        }

        void MovePlayer()
        {
            int x = 0;
            int y = 0;

            switch (currentDirection)
            {
                case Direction.up:
                    y = 1;
                    break;
                case Direction.down:
                    y = -1;
                    break;
                case Direction.left:
                    x = -1;
                    break;
                case Direction.right:
                    x = 1;
                    break;
            }

            Node targetNode = GetNode(playerNode.x + x, playerNode.y + y);
            if (targetNode == null)
            {
                //game over
                onGameOver.Invoke();
            }
            else
            {
                if (akoUdari(targetNode))
                {
                    //game over
                    onGameOver.Invoke();
                }
                else
                {
                    bool pogodak = false;

                    if (targetNode == appleNode)
                    {
                        pogodak = true;
                    }

                    Node previousNode = playerNode;

                    availableNodes.Add(previousNode);

                    if (pogodak)
                    {
                        tail.Add(CreateTailNode(previousNode.x, previousNode.y));
                        availableNodes.Remove(previousNode);
                    }

                    MoveTail();
                    //move tail

                    PlacePlayerObject(playerObj, targetNode.worldPosition);
                    playerNode = targetNode;
                    availableNodes.Remove(playerNode);

                    if (pogodak)
                    {
                        DBManager.score++;
                        curScore++;
                        if (curScore >= highScore)
                        {
                            highScore = curScore;
                        }

                        onScore.Invoke();

                        if (availableNodes.Count > 0)
                        {
                            RandomlyPlaceApple();
                        }
                        else
                        {
                            // you won!
                        }
                    }
                }

            }
        }

        void MoveTail()
        {
            Node prevNode = null;
            for (int i = 0; i < tail.Count; i++)
            {
                SpecialNode p = tail[i];
                availableNodes.Add(p.node);

                if (i == 0)
                {
                    prevNode = p.node;
                    p.node = playerNode;
                }
                else
                {
                    Node prev = p.node;
                    p.node = prevNode;
                    prevNode = prev;
                }

                availableNodes.Remove(p.node);
                PlacePlayerObject(p.obj, p.node.worldPosition); //centriranje

            }
        }
        #endregion
        //-----------------END UPDATE


        //-----------------UTILITIES
        #region Utilities

        public void GameOver()
        {
            isGameOver = true;
            isFirstInput = false;
        }

        public void UpdateScore()
        {
            currentScoreText.text = curScore.ToString();
            highScoreText.text = highScore.ToString();
        }

        bool akoJeSupprotno(Direction d)
        {
            switch (d)
            {
                default:
                case Direction.up:
                    if (currentDirection == Direction.down)
                        return true;
                    else
                        return false;
                case Direction.down:
                    if (currentDirection == Direction.up)
                        return true;
                    else
                        return false;
                case Direction.left:
                    if (currentDirection == Direction.right)
                        return true;
                    else
                        return false;
                case Direction.right:
                    if (currentDirection == Direction.left)
                        return true;
                    else
                        return false;
            }

        }

        bool akoUdari(Node n)
        {
            for (int i = 0; i < tail.Count; i++)
            {
                if (tail[i].node == n)
                {
                    return true;
                }
            }
            return false;
        }

        void PlacePlayerObject(GameObject obj, Vector3 pos)
        {
            pos += Vector3.one * .5f;
            obj.transform.position = pos;
        }

        void RandomlyPlaceApple()
        {
            int ran = Random.Range(0, availableNodes.Count);
            Node n = availableNodes[ran];
            PlacePlayerObject(appleObj, n.worldPosition);
            appleNode = n;
        }

        Node GetNode(int x, int y)
        {
            if (x < 0 || x > maxWidth - 1 || y < 0 || y > maxHeight - 1)
                return null;

            return grid[x, y];
        }

        SpecialNode CreateTailNode(int x, int y)
        {
            SpecialNode s = new SpecialNode();
            s.node = GetNode(x, y);
            s.obj = new GameObject();
            s.obj.transform.parent = tailParent.transform;
            s.obj.transform.position = s.node.worldPosition;
            s.obj.transform.localScale = Vector3.one * 0.7f;
            SpriteRenderer r = s.obj.AddComponent<SpriteRenderer>();
            r.sprite = playerSprite;
            r.sortingOrder = 1;
            return s;
        }

        Sprite CreateSprite(Color targetColor)
        {
            Texture2D txt = new Texture2D(1, 1);
            txt.SetPixel(0, 0, targetColor);
            txt.filterMode = FilterMode.Point;
            txt.Apply();
            Rect rect = new Rect(0, 0, 1, 1);
            return Sprite.Create(txt, rect, Vector2.one *.5f, 1, 0, SpriteMeshType.FullRect);
        }

        #endregion  
        //-----------------END UTILITES
    }
}