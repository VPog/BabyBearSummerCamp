using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;

public class BearController : MonoBehaviour
{
    public GameObject bearManager;
    private bool selected;
    public bool Selected
    {
        get => selected;
        set
        {
            selected = value;
            Highlight(value);
        }
    }
    
    //Timer for the minute function - Is in seconds. 
    private const int TIMER = 60;
    //Odds that there will be a reduction in a stat each minute.
    //This SHOULD NOT be less than 3 - or else unintended results will occur!
    private const int RANDOM_CHANCE = 10;
    
    //The main stats for the bears
    private float happiness = 5;
    private float hunger = 5;
    private float energy = 5;

    //Bear Movement
    public float moveSpeed = 5f;
    public Transform movePoint;
    public LayerMask stopsMovement;


    void Start()
    {
        //unparent movePoint with bear so it doesn't follow bear's movement
        movePoint.parent = null;

        //Sets the temperment 
        temperament = new Temperament(TEMPERAMENTS[(int)Random.Range(0, TEMPERAMENTS.Length)]);

        UpdateText();
        StartCoroutine(MinuteUpdate());
    }

    void Update()
    {
        if (PathToFollow != null && PathToFollow.Count != 0)
        {
            // Follow last item in path
            BearManager bm = bearManager.GetComponent<BearManager>();
            Tilemap tm = bm.mainTilemap.GetComponent<Tilemap>();
            movePoint.position = bm.TilePosToWorldPos(tm, PathToFollow[PathToFollow.Count-1]);
            if (Mathf.Abs(transform.position.x - movePoint.position.x) < tm.cellSize.x
                && Mathf.Abs(transform.position.y - movePoint.position.y) < tm.cellSize.y)
            {
                PathToFollow.RemoveAt(PathToFollow.Count - 1);
            }
        }

        //determine the place where the position is indicated by the user
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);


        //make sure the player really want to change the move position
        if (Vector3.Distance(transform.position, movePoint.position) <= .05f)
        {
            //two if statements: allow for diagonal movements 
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f)
            {
                Vector3 moveVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
                if (!Physics2D.OverlapCircle(movePoint.position + moveVector, .2f, stopsMovement))
                {
                    movePoint.position += moveVector;
                }
            }

            if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f)
            {
                Vector3 moveVector = new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);
                if (!Physics2D.OverlapCircle(movePoint.position + moveVector, .2f, stopsMovement))
                {
                    movePoint.position += moveVector;
                }
            }
        }

    }

    private Temperament temperament;

    // Current pathfinding path, if any
    public IList<Vector2Int> PathToFollow = null;

    //The GUI connected to a bearObject for testing purposes
    //public TextMeshProUGUI bearText;

    //Static Array containing every single temperment, currently only contains two temperments 
    static readonly Temperament[] TEMPERAMENTS = new Temperament[]
    {
        
        new Temperament("Moody", 0.8f, 1, 1),
        new Temperament("Bubbly", 1.2f, 1, 1)

    };

    
    //These are called properties - There are essentially streamlined getter 
    //and setter functions that you can access with pretty UI
    //Example: Bear.Happiness instead of Bear.getHappiness
    //TODO: Rework the temperament formula into one that makes more sense! 
    //Look into reworking the quantity of the stats themselves - Meet with the team for this!
    public float Happiness
    {
        get => happiness;
        set
        {
            //If there is a positive increase, modify the happiness gain by the bear's temperament
            if (value > 0)
            {
                happiness = value * temperament.HappinessMod;
            }
            else
            {
                happiness = value * temperament.HappinessMod; 
            }
            UpdateText();
        }
    }

    public float Hunger
    {
        get => hunger;
        set
        {
            hunger = value * temperament.HungerMod;
            UpdateText();
        }
    }

    public float Energy
    {
        get => energy;
        set
        {
            energy = value * temperament.EnergyMod;
            UpdateText();
        }
    }

    #region Unity Methods

    #endregion

    // Show or hide the highlight that shows below the bear when it is selected
    public void Highlight(bool highlight)
    {
        transform.Find("Highlight").GetComponent<SpriteRenderer>().enabled = highlight;
    }

#region Bear Stats




private void GenerateTemperment()
    {
    }

    // Calls once every minute 
    IEnumerator MinuteUpdate()
    {
        while (true)
        {
            Debug.Log("Starting the Minute Downtime");
            yield return new WaitForSeconds(TIMER);
            
            RandomReduction();
        }
    }
    
    //This function randomly reduces one of the bear's stats
    void RandomReduction()
    {
        int randNum = Random.Range(1, RANDOM_CHANCE);
        switch (randNum)
        {
            case 1:
                Happiness -= 1;
                break;
            case 2:
                Hunger -= 1;
                break;
            case 3:
                Energy -= 1;
                break;
            default:
                Debug.Log("No Value was reduced.");
                break;
        }
    }
    
    //Temp Function that updates a text UI element 
    void UpdateText()
    {
        // bearText.text = "I am a bear!\nHappiness: " + Happiness + "\nHunger: " + Hunger + "\nSleepiness: " + Energy;

    }
    #endregion

    #region Pathfinding

    public void OnMouseDown()
    {
        var b = bearManager.GetComponent<BearManager>();
        if (!b.shiftPressed)
        {
            b.DeselectAllBears();
        }
        b.SelectBear(this);
    }

    #endregion


}
