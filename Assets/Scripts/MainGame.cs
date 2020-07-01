using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPSBotsLib;

public class MainGame : MonoBehaviour
{
    private Vector3 planestartpos;
    private float y_startdistfromplane;

    public GameObject agentemplate;
    public GameObject walltemplate;
    public GameObject itemtemplate;
    public GameObject texttemplate;
    public GameObject arrowtemplate;
    public GameObject itemcollectedparticle;
    public GameObject laserbullettemplate;

    private GameModel model;
    private List<GameObject> maze;
    private List<GameObject> agents;
    private List<GameObject> agenttexts;
    private List<GameObject> agentarrows;
    private List<GameObject> items;
    private List<int> itemticks;
    private List<GameObject> itemparticles;
    private List<Vector3> agentstartpos;
    private List<Vector3> walls;

    private int agentnum;
    private int agentcounter;
    private int framecounter;
    private int iterationcounter;
    public int waitframes;
    public float agenttext_rotation;
    public float itemthreshold;
    public int displaceitemby;
    public float agentwallthreshold;
    public float bulletspeed;

    private Color healthgreen;
    private Color healthyellow;
    private Color healthred;

    private int itemcount;
    private int hitcount;
    private int misscount;
    private int killcount;

    private bool stilltraining;
    private bool stillreplaying;

    public GameModel Model
    {
        get { return model; }
    }
    public int AgentCounter
    {
        get { return agentcounter; }
    }
    public int ItemCount
    {
        get { return itemcount; }
    }
    public int HitCount
    {
        get { return hitcount; }
    }
    public int MissCount
    {
        get { return misscount; }
    }
    public int KillCount
    {
        get { return killcount; }
    }
    public int IterationCounter
    {
        get { return iterationcounter; }
    }
    public bool StillTraining
    {
        get { return stilltraining; }
    }
    public bool StillReplaying
    {
        get { return stillreplaying; }
    }

    void Start()
    {
        stilltraining = true;
        stillreplaying = false;

        healthgreen = Color.green;
        healthyellow = Color.yellow;
        healthred = Color.red;

        agentcounter = 0;
        framecounter = 0;
        iterationcounter = 0;

        itemcount = 0;
        hitcount = 0;
        misscount = 0;
        killcount = 0;
    }

    void Update()
    {
        if (stilltraining)
        {
            model = new GameModel();
            model.TrainAllControllers();

            stilltraining = false;
            stillreplaying = true;

            y_startdistfromplane = 0.2f;

            Vector3 planeSize = this.GetComponent<Renderer>().bounds.size;
            float mazesize = 50.0f;
            planestartpos = new Vector3(
                this.transform.position.x - (planeSize.x / 2.0f) + ((planeSize.x - mazesize) / 2.0f),
                y_startdistfromplane,
                this.transform.position.z - (planeSize.z / 2.0f) + ((planeSize.z - mazesize) / 2.0f));

            StartView();

            agentnum = agents.Count;
        }
        else
        {
            if (framecounter == 0)
            {
                if (agentcounter < model.UltimateData.Agentdata.Count)
                {
                    for (int i = 0; i < agentnum; i++)
                    {
                        // Change agent's position
                        if (model.UltimateData.Agentdata[agentcounter + i].action == Action.movefw ||
                            model.UltimateData.Agentdata[agentcounter + i].action == Action.movebw)
                        {
                            float nextX = planestartpos.x + (float)(model.UltimateData.Agentdata[agentcounter + i].pos.X);
                            float nextZ = planestartpos.z + (float)(model.UltimateData.Agentdata[agentcounter + i].pos.Y);

                            // Avoid passing into walls
                            bool foundclosewall = false;
                            int j = 0;
                            while (j < walls.Count && !foundclosewall)
                            {
                                if (Mathf.Abs(walls[j].x - nextX) < agentwallthreshold &&
                                    Mathf.Abs(walls[j].z - nextZ) < agentwallthreshold)
                                    foundclosewall = true;
                                else
                                    j++;
                            }

                            if (!foundclosewall)
                            {
                                // Step
                                agents[i].transform.position = new Vector3(nextX, agents[i].transform.position.y, nextZ);

                                agenttexts[i].transform.position = new Vector3(nextX, agenttexts[i].transform.position.y, nextZ);
                                agentarrows[i].transform.position = new Vector3(nextX - 0.2f, agentarrows[i].transform.position.y, nextZ);
                            }
                            // Else avoid the wall
                            // In model data, agent moved  because it was valid
                            // but here it should not be displayed if it is overlapping a wall

                        }
                        // Change agent's direction
                        else if (model.UltimateData.Agentdata[agentcounter + i].action == Action.turnright)
                        {
                            agents[i].transform.Rotate(0f, 5f, 0f);
                            agentarrows[i].transform.Rotate(0f, 5f, 0f);
                        }
                        else if (model.UltimateData.Agentdata[agentcounter + i].action == Action.turnleft)
                        {
                            agents[i].transform.Rotate(0f, -5f, 0f);
                            agentarrows[i].transform.Rotate(0f, -5f, 0f);
                        }
                        // If item was picked up
                        else if (model.UltimateData.Agentdata[agentcounter + i].action == Action.item)
                        {
                            // Displace item object (out of sight)
                            int itemindex = -1;
                            for (int j = 0; j < items.Count; j++)
                            {
                                if (Vector3.Distance(agents[i].transform.position, items[j].transform.position) <= itemthreshold)
                                    itemindex = j;
                            }
                            if (itemindex >= 0 && itemindex < items.Count)
                            {
                                items[itemindex].transform.position = new Vector3(
                                    items[itemindex].transform.position.x,
                                    items[itemindex].transform.position.y - displaceitemby,
                                    items[itemindex].transform.position.z);

                                // Start decreasing respawn tick
                                itemticks[itemindex]--;

                                // Play item collected particle
                                var particlesystem = itemparticles[itemindex].GetComponent<ParticleSystem>();
                                particlesystem.Play();
                            }

                            itemcount++;
                        }
                        // If agent shoots
                        else if (model.UltimateData.Agentdata[agentcounter + i].action == Action.shoot)
                        {
                            GameObject laserbullet = MainGame.Instantiate(laserbullettemplate, new Vector3(
                                agents[i].transform.position.x,
                                1,
                                agents[i].transform.position.z),
                                "laserbullet");

                            float yRotation = agents[i].transform.eulerAngles.y;
                            float zRotation = agents[i].transform.eulerAngles.z;
                            laserbullet.transform.Rotate(0, yRotation, zRotation);

                            var lbbody = laserbullet.GetComponent<Rigidbody>();
                            lbbody.velocity = lbbody.transform.forward * bulletspeed;
                        }

                        // Update agent's health
                        int currenthealth = model.UltimateData.Agentdata[agentcounter + i].health;
                        var textmesh = (agenttexts[i]).GetComponent<TextMesh>();
                        textmesh.text = currenthealth.ToString();
                        if (currenthealth > 50)
                            textmesh.color = healthgreen;
                        else if (currenthealth <= 50 && currenthealth > 20)
                            textmesh.color = healthyellow;
                        else
                            textmesh.color = healthred;

                        // Stats
                        if (model.UltimateData.Agentdata[agentcounter + i].reward_miss)
                            misscount++;
                        if (model.UltimateData.Agentdata[agentcounter + i].reward_hit)
                            hitcount++;
                        if (model.UltimateData.Agentdata[agentcounter + i].reward_kill)
                            killcount++;
                    }

                    // Decrease item respawn ticks if needed
                    for (int i = 0; i < itemticks.Count; i++)
                    {
                        if (itemticks[i] < model.ItemRespawn)
                            itemticks[i]--;

                        if (itemticks[i] <= 0)
                        // Respawn item object
                        {
                            itemticks[i] = model.ItemRespawn;
                            items[i].transform.position = new Vector3(
                                    items[i].transform.position.x,
                                    items[i].transform.position.y + displaceitemby,
                                    items[i].transform.position.z);

                            // Play item collected particle
                            var particlesystem = itemparticles[i].GetComponent<ParticleSystem>();
                            particlesystem.Play();
                        }
                    }

                    // Clean up bullet explosion objects
                    GameObject[] explosions = GameObject.FindGameObjectsWithTag("explosion");
                    foreach (GameObject explosion in explosions)
                    {
                        var explosionsystem = explosion.GetComponent<ParticleSystem>();
                        if (!(explosionsystem.isPlaying))
                            Destroy(explosion.gameObject);
                    }

                    agentcounter += agentnum;
                    framecounter++;
                    iterationcounter++;
                }
                else
                {
                    // Finish replay
                    stillreplaying = false;
                }
            }
            else if (framecounter > 0 && framecounter < waitframes)
                framecounter++;
            else
                framecounter = 0;
        }
    }

    public static GameObject Instantiate(GameObject prefab, Vector3 position, string tag)
    {
        var gameobject = GameObject.Instantiate(prefab, position, Quaternion.identity) as GameObject;
        gameobject.tag = tag;
        return gameobject;
    }

    private GameObject GetTemplateForField(FieldType field, Vector3 pos)
    {
        GameObject obj = new GameObject();

        if (field == FieldType.Wall)
        {
            obj = MainGame.Instantiate(walltemplate, new Vector3(pos.x, pos.y + (1 - y_startdistfromplane), pos.z), "wall");
        }
        else if (field == FieldType.Item)
        {
            obj = MainGame.Instantiate(itemtemplate, pos, "item");
        }
        return obj;
    }

    private GameObject GetTemplateForAgent(Vector3 pos)
    {
        GameObject obj = MainGame.Instantiate(agentemplate, pos, "agent");
        return obj;
    }

    private void StartView()
    {
        // Get data to replay Ultimate (walls, items)
        maze = new List<GameObject>();
        agents = new List<GameObject>();
        items = new List<GameObject>();
        itemticks = new List<int>();
        itemparticles = new List<GameObject>();
        agentstartpos = new List<Vector3>();
        walls = new List<Vector3>();
        agenttexts = new List<GameObject>();
        agentarrows = new List<GameObject>();

        for (int i = 0; i < model.Ultimatemap.Count; i++)
        {
            for (int j = 0; j < model.Ultimatemap[i].Count; j++)
            {
                Vector3 currentpos = new Vector3(planestartpos.x + i, planestartpos.y, planestartpos.z + j);
                GameObject currentobj = GetTemplateForField(model.Ultimatemap[i][j], currentpos);

                if (model.Ultimatemap[i][j] == FieldType.Item)
                {
                    currentobj.transform.Rotate(0f, 0f, -90f);
                    items.Add(currentobj);
                    itemticks.Add(model.ItemRespawn);
                    
                    GameObject particle = MainGame.Instantiate(itemcollectedparticle, currentpos, "itemparticle");
                    particle.transform.Rotate(new Vector3(-90f, 0f, 0f));
                    itemparticles.Add(particle);
                    var particlesystem = particle.GetComponent<ParticleSystem>();
                    particlesystem.Stop();
                }
                else if (model.Ultimatemap[i][j] == FieldType.Wall)
                {
                    walls.Add(currentpos);
                }

                maze.Add(currentobj);
            }
        }
    
    
        // Put down agents
        for (int i = 0; i < model.Agentsinit_ultimate.Count; i++)
        {
            Vector3 currentpos = new Vector3(planestartpos.x + (float)(model.Agentsinit_ultimate[i].X),
                0.5f,
                planestartpos.z + (float)(model.Agentsinit_ultimate[i].Y));
            GameObject currentagent = GetTemplateForAgent(currentpos);

            agents.Add(currentagent);
            agentstartpos.Add(currentpos);

            GameObject textobj = MainGame.Instantiate(texttemplate, 
                new Vector3(currentpos.x, 2, currentpos.z), 
                "agenttext");
            textobj.transform.Rotate(agenttext_rotation, 0f, 0f);
            var textmesh = textobj.GetComponent<TextMesh>();
            textmesh.text = "100";
            textmesh.color = healthgreen;
            agenttexts.Add(textobj);

            GameObject arrowobj = MainGame.Instantiate(arrowtemplate,
                new Vector3(currentpos.x - 0.2f, 2.2f, currentpos.z),
                "arrow");
            arrowobj.transform.Rotate(0f, 90f, 0f);
            agentarrows.Add(arrowobj);
        }
    }
}
