using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;

namespace FPSBotsLib
{
    public class Position
    {
        private Double x;
        private Double y;

        public Double X
        {
            get { return x; }
            set { }
        }

        public Double Y
        {
            get { return y; }
            set { }
        }

        public Position()
        {
            x = 0;
            y = 0;
        }

        public Position(Double _x, Double _y)
        {
            x = _x;
            y = _y;
        }
    }

    public enum FieldType
    {
        Empty,
        Wall,
        Item
    }

    public enum FieldBehaviour
    {
        stealth2,       // wall  (1)
        stealth1,       // item  (2)
        aggressive1,    // empty (3)
        aggressive2     // enemy (4)
    }

    public class Configuration
    {

        // input
        public String configfilename;
        public String map_file_nav;
        public String map_file_nav2;
        public String map_file_com;
        public String map_ultimate;

        // gameplay
        public Int32 mapsize;
        public Int32 botnum;
        public Int32 agent_viewrange;
        public Int32 agent_health;
        public Int32 tick_cooldown;
        public Int32 tick_itemrespawn;
        public Int32 medkit;
        public Int32 min_degreeturn;
        public Int32 max_degreeturn;
        public Double collisiontreshold;
        public Int32 enemy_close;
        public Int32 enemy_far;
        public Int32 enemy_hit;
        public Int32 localenv;

        // output


        // reward_nav
        public Double rewardnav_collision;
        public Double rewardnav_moving;
        public Double rewardnav_item;
        // reward_comb
        public Double rewardcomb_hit;
        public Double rewardcomb_kill;
        public Double rewardcomb_miss;
        public Double rewardcomb_killed;
        public Double rewardcomb_wounded;

        // sarsa
        public Double gamma;
        public Double alphainit;
        public Double alphatarget;
        public Double lambda;
        public Double epsilon;
        public Int32 iterationlimit;

        public Configuration(String _configfilename)
        {
            configfilename = _configfilename;

            try
            {
                XDocument doc = XDocument.Load(configfilename);

                map_file_nav = doc.Element("parameters").Element("input").Element("map_file_nav").Value;
                map_file_nav2 = doc.Element("parameters").Element("input").Element("map_file_nav2").Value;
                map_file_com = doc.Element("parameters").Element("input").Element("map_file_com").Value;
                map_ultimate = doc.Element("parameters").Element("input").Element("map_ultimate").Value;

                mapsize = Int32.Parse(doc.Element("parameters").Element("gameplay").Element("mapsize").Value);
                botnum = Int32.Parse(doc.Element("parameters").Element("gameplay").Element("botnum").Value);
                agent_viewrange = Int32.Parse(doc.Element("parameters").Element("gameplay").Element("agent_viewrange").Value);
                agent_health = Int32.Parse(doc.Element("parameters").Element("gameplay").Element("agent_health").Value);
                tick_cooldown = Int32.Parse(doc.Element("parameters").Element("gameplay").Element("tick_cooldown").Value);
                tick_itemrespawn = Int32.Parse(doc.Element("parameters").Element("gameplay").Element("tick_itemrespawn").Value);
                medkit = Int32.Parse(doc.Element("parameters").Element("gameplay").Element("medkit").Value);
                min_degreeturn = Int32.Parse(doc.Element("parameters").Element("gameplay").Element("min_degreeturn").Value);
                max_degreeturn = Int32.Parse(doc.Element("parameters").Element("gameplay").Element("max_degreeturn").Value);
                collisiontreshold = Double.Parse(doc.Element("parameters").Element("gameplay").Element("collisiontreshold").Value);
                enemy_close = Int32.Parse(doc.Element("parameters").Element("gameplay").Element("enemy_close").Value);
                enemy_far = Int32.Parse(doc.Element("parameters").Element("gameplay").Element("enemy_far").Value);
                enemy_hit = Int32.Parse(doc.Element("parameters").Element("gameplay").Element("enemy_hit").Value);
                localenv = Int32.Parse(doc.Element("parameters").Element("gameplay").Element("localenv").Value);

                rewardnav_collision = Double.Parse(doc.Element("parameters").Element("reward_nav").Element("collision").Value);
                rewardnav_moving = Double.Parse(doc.Element("parameters").Element("reward_nav").Element("moving").Value);
                rewardnav_item = Double.Parse(doc.Element("parameters").Element("reward_nav").Element("item").Value);
                rewardcomb_hit = Double.Parse(doc.Element("parameters").Element("reward_comb").Element("hit").Value);
                rewardcomb_kill = Double.Parse(doc.Element("parameters").Element("reward_comb").Element("kill").Value);
                rewardcomb_miss = Double.Parse(doc.Element("parameters").Element("reward_comb").Element("miss").Value);
                rewardcomb_killed = Double.Parse(doc.Element("parameters").Element("reward_comb").Element("killed").Value);
                rewardcomb_wounded = Double.Parse(doc.Element("parameters").Element("reward_comb").Element("wounded").Value);


                gamma = Double.Parse(doc.Element("parameters").Element("sarsa").Element("gamma").Value);
                alphainit = Double.Parse(doc.Element("parameters").Element("sarsa").Element("alphainit").Value);
                alphatarget = Double.Parse(doc.Element("parameters").Element("sarsa").Element("alphatarget").Value);
                lambda = Double.Parse(doc.Element("parameters").Element("sarsa").Element("lambda").Value);
                epsilon = Double.Parse(doc.Element("parameters").Element("sarsa").Element("epsilon").Value);
                iterationlimit = Int32.Parse(doc.Element("parameters").Element("sarsa").Element("iterationlimit").Value);

                map_file_nav = map_file_nav.TrimStart(new char[] { '\r', '\n', '\t' });
                map_file_nav2 = map_file_nav2.TrimStart(new char[] { '\r', '\n', '\t' });
                map_file_com = map_file_com.TrimEnd(new char[] { '\r', '\n', '\t' });
                map_ultimate = map_ultimate.TrimEnd(new char[] { '\r', '\n', '\t' });
            }
            catch (System.IO.FileNotFoundException)
            {
                //Console.WriteLine("FileNotFoundException while reading Configuration");
                //Console.ReadKey();
            }
            catch (NullReferenceException)
            {
                //Console.WriteLine("NullReferenceException while reading Configuration");
                //Console.ReadKey();
            }
        }

        public Configuration(Configuration c)
        {
            if (c == null)
                return;

            configfilename = c.configfilename;
            map_file_nav = c.map_file_nav;
            map_file_nav2 = c.map_file_nav2;
            map_file_com = c.map_file_com;
            map_ultimate = c.map_ultimate;

            mapsize = c.mapsize;
            botnum = c.botnum;
            agent_viewrange = c.agent_viewrange;
            agent_health = c.agent_health;
            tick_cooldown = c.tick_cooldown;
            tick_itemrespawn = c.tick_itemrespawn;
            medkit = c.medkit;
            min_degreeturn = c.min_degreeturn;
            max_degreeturn = c.max_degreeturn;
            collisiontreshold = c.collisiontreshold;
            enemy_close = c.enemy_close;
            enemy_far = c.enemy_far;
            enemy_hit = c.enemy_hit;
            localenv = c.localenv;

            rewardnav_collision = c.rewardnav_collision;
            rewardnav_moving = c.rewardnav_moving;
            rewardnav_item = c.rewardnav_item;

            rewardcomb_hit = c.rewardcomb_hit;
            rewardcomb_kill = c.rewardcomb_kill;
            rewardcomb_miss = c.rewardcomb_miss;
            rewardcomb_killed = c.rewardcomb_killed;
            rewardcomb_wounded = c.rewardcomb_wounded;

            gamma = c.gamma;
            alphainit = c.alphainit;
            alphatarget = c.alphatarget;
            lambda = c.lambda;
            epsilon = c.epsilon;
            iterationlimit = c.iterationlimit;
        }
    }

    public class AgentData
    {
        public Int32 id;
        public Int32 iteration;
        public Position pos;
        public Action action;
        public Int32 health;
        public Int32 timestamp;
        public Boolean reward_miss;
        public Boolean reward_hit;
        public Boolean reward_kill;

        public AgentData(Int32 _id, Int32 _it, Position _pos, Action _action, Int32 _health, Int32 _timestamp, Boolean _miss, Boolean _hit, Boolean _kill)
        {
            id = _id;
            iteration = _it;
            pos = new Position(_pos.X, _pos.Y);
            action = _action;
            health = _health;
            timestamp = _timestamp;
            reward_miss = _miss;
            reward_hit = _hit;
            reward_kill = _kill;
        }
    }

    public class Statistics
    {
        // For all training
        private List<Int32> collisioncounts;
        private List<Double> distancestravelled;
        private List<Int32> itemcounts;
        private List<Int32> killCounts;
        private List<Int32> deathCounts;
        private List<Tuple<Int32, Double>> localenv_values;
        private List<Int32> hitCounts;
        private List<Int32> missCounts;

        // For replay
        private List<AgentData> agentdata;

        public Statistics(Int32 botnum)
        {
            collisioncounts = new List<Int32>();
            distancestravelled = new List<Double>();
            itemcounts = new List<Int32>();
            killCounts = new List<Int32>();
            deathCounts = new List<Int32>();
            localenv_values = new List<Tuple<Int32, Double>>();
            hitCounts = new List<Int32>();
            missCounts = new List<Int32>();
            agentdata = new List<AgentData>();

            for (int i = 0; i < botnum; i++)
            {
                collisioncounts.Add(0);
                distancestravelled.Add(0.0);
                itemcounts.Add(0);
                killCounts.Add(0);
                deathCounts.Add(0);
                hitCounts.Add(0);
                missCounts.Add(0);
            }
        }

        public List<AgentData> Agentdata
        {
            get { return agentdata; }
            set { }
        }
        
        public void UpdateStats(Int32 agentid, Boolean collision, Double newdistance, Boolean item, Int32 iteration, Position pos, 
            Action action, Int32 health, Int32 timestamp, List<Reward> rewards)
        {
            if (agentid >= collisioncounts.Count || agentid >= distancestravelled.Count || agentid >= itemcounts.Count)
                return;
            else
            {
                if (collision)
                    collisioncounts[agentid]++;

                if (newdistance > 0)
                    distancestravelled[agentid] += newdistance;

                if (item)
                    itemcounts[agentid]++;

                Boolean miss = rewards.Contains(Reward.miss);
                Boolean hit = rewards.Contains(Reward.hit);
                Boolean kill = rewards.Contains(Reward.kill);

                agentdata.Add(new AgentData(agentid, iteration, new Position(pos.X, pos.Y), action, health, timestamp, miss, hit, kill));
            }
        }

        public void UpdateKD(Int32 agentid, Int32 kills, Int32 deaths)
        {
            if (agentid >= killCounts.Count || agentid >= deathCounts.Count)
                return;
            else
            {
                if (killCounts[agentid] != kills)
                    killCounts[agentid] = kills;
                if (deathCounts[agentid] != deaths)
                    deathCounts[agentid] = deaths;
            }
        }

        public void UpdateHitMiss(Int32 agentid, Reward reward)
        {
            if (agentid >= hitCounts.Count || agentid >= missCounts.Count)
                return;
            else
            {
                if (reward == Reward.hit)
                    hitCounts[agentid]++;
                else if (reward == Reward.miss)
                    missCounts[agentid]++;
            }
        }

        public void UpdateLocalEnvValues(Int32 agentid, Double value)
        {
            localenv_values.Add(new Tuple<Int32, Double>(agentid, value));
        }

        public void WriteStat_Navigation(String outputfolder, String file1, String file2, Int32 botnum)
        {
            using (StreamWriter sw = File.CreateText(outputfolder + file1))
            {
                sw.Write("AgentID;Collision;Distance;Items;\n");
                for (int i = 0; i < collisioncounts.Count; i++)
                {
                    sw.Write(i + ";" + collisioncounts[i] + ";" + Math.Round(distancestravelled[i], 4) + ";" + itemcounts[i] + ";\n");
                }
            }

            //Console.WriteLine("Stats file written.");

            for (int b = 0; b < botnum; b++)
            {
                using (StreamWriter sw = File.CreateText(outputfolder + "agent" + b.ToString() + "_" + file2))
                {
                    sw.Write("Iteration;X;Y;Action;\n");
                    for (int i = 0; i < agentdata.Count; i++)
                    {
                        if (agentdata[i].id == b)
                            sw.Write(agentdata[i].iteration + ";" + agentdata[i].pos.X + ";" + agentdata[i].pos.Y + ";" + agentdata[i].action + ";\n");
                    }
                }

                //Console.WriteLine("Agentdata" + b.ToString() + " file written.");
            }
        }

        public void WriteStat_Combat(String outputfolder, String file1, String file2, Int32 botnum)
        {
            using (StreamWriter sw = File.CreateText(outputfolder + file1))
            {
                sw.Write("AgentID;Collision;Distance;Kills;Deaths;Hits;Misses;\n");
                for (int i = 0; i < collisioncounts.Count; i++)
                {
                    sw.Write(i + ";" + collisioncounts[i] + ";" + Math.Round(distancestravelled[i], 4) + ";" +
                        killCounts[i] + ";" + deathCounts[i] + ";" + hitCounts[i] + ";" + missCounts[i] + ";\n");
                }
            }

            //Console.WriteLine("Stats file written.");

            for (int b = 0; b < botnum; b++)
            {
                using (StreamWriter sw = File.CreateText(outputfolder + "agent" + b.ToString() + "_" + file2))
                {
                    sw.Write("Iteration;X;Y;Action;Timestamp;\n");
                    for (int i = 0; i < agentdata.Count; i++)
                    {
                        if (agentdata[i].id == b)
                            sw.Write(agentdata[i].iteration + ";" + agentdata[i].pos.X + ";" + agentdata[i].pos.Y + ";" +
                                agentdata[i].action + ";" + agentdata[i].timestamp + ";\n");
                    }
                }

                //Console.WriteLine("Agentdata" + b.ToString() + " file written.");
            }
        }

        public void WriteStat_Ultimate(String outputfolder, String file1, String file2, String file3, Int32 botnum)
        {
            using (StreamWriter sw = File.CreateText(outputfolder + file1))
            {
                sw.Write("AgentID;Collision;Distance;Items;Kills;Deaths;Hits;Misses;\n");
                for (int i = 0; i < collisioncounts.Count; i++)
                {
                    sw.Write(i + ";" + collisioncounts[i] + ";" + Math.Round(distancestravelled[i], 4) + ";" + itemcounts[i] + ";" +
                        killCounts[i] + ";" + deathCounts[i] + ";" + hitCounts[i] + ";" + missCounts[i] + ";\n");
                }
            }

            //Console.WriteLine("Stats file written.");

            using (StreamWriter sw = File.CreateText(outputfolder + file3))
            {
                sw.Write("AgentID;LocalEnvValue;\n");
                for (int i = 0; i < localenv_values.Count; i++)
                {
                    sw.Write(localenv_values[i].Item1 + ";" + localenv_values[i].Item2 + ";\n");
                }
            }

            //Console.WriteLine("Local Env Values file written.");

            for (int b = 0; b < botnum; b++)
            {
                using (StreamWriter sw = File.CreateText(outputfolder + "agent" + b.ToString() + "_" + file2))
                {
                    sw.Write("Iteration;X;Y;Action;\n");
                    for (int i = 0; i < agentdata.Count; i++)
                    {
                        if (agentdata[i].id == b)
                            sw.Write(agentdata[i].iteration + ";" + agentdata[i].pos.X + ";" + agentdata[i].pos.Y + ";" + agentdata[i].action + ";\n");
                    }
                }

                //Console.WriteLine("Agentdata" + b.ToString() + " file written.");
            }
        }

        public void Visuals(List<List<FieldType>> map, Int32 size, Int32 botnum, String outputfolder, String file)
        {
            using (StreamWriter sw = File.CreateText(outputfolder + file))
            {
                for (int b = 0; b < botnum; b++)
                {
                    sw.Write("Agent " + (b) + " heatmap;\n");

                    // Initialize heatmap
                    List<List<Int32>> heatmap = new List<List<Int32>>();
                    for (int i = 0; i < size; i++)
                    {
                        List<Int32> onerow = new List<Int32>();
                        for (int j = 0; j < size; j++)
                        {
                            if (map[i][j] == FieldType.Wall)
                                onerow.Add(-100);
                            else
                                onerow.Add(0);
                        }

                        heatmap.Add(onerow);
                    }

                    // Get heatmap from agentdata
                    for (int i = 0; i < agentdata.Count; i++)
                    {
                        if (agentdata[i].id == b)
                            heatmap[(int)(agentdata[i].pos.X)][(int)(agentdata[i].pos.Y)]++;
                    }

                    // Output heatmap
                    for (int j = 0; j < heatmap.Count; j++)
                    {
                        for (int k = 0; k < heatmap[j].Count; k++)
                        {
                            sw.Write(heatmap[j][k] + ";");
                        }
                        sw.Write("\n");
                    }

                    sw.Write("\n\n");
                }
            }

            //Console.WriteLine("Heatmaps written for Ultimate.");
        }
    }
}
