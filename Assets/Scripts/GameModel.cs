using System;
using System.Collections.Generic;

namespace FPSBotsLib
{
    public class GameModel
    {
        private Configuration config;
        private String inputfolder;
        private String outputfolder;
        private List<List<Int32>> rawdata;
        private List<List<FieldType>> map;
        private List<Position> agentsinit_nav;
        private List<Position> agentsinit_com;
        private List<Tuple<Position, Int32>> items;

        private Agents agents;
        private Int32 loopcount;

        private Statistics stat_nav;
        private Statistics stat_nav2;
        private Statistics stat_com;

        // Save for replay -- ultimate is last in cycle so no need to save those (map, agentsinit_ultimate, items_ultimate)
        private List<List<FieldType>> arenamap;
        private List<List<FieldType>> mazemap;
        private List<List<FieldType>> combatmap;
        private List<List<FieldType>> ultimatemap;
        private List<Position> agentsinit_arena;
        private List<Position> agentsinit_maze;
        private List<Position> agentsinit_combat;
        private List<Tuple<Position, Int32>> items_arena;
        private List<Tuple<Position, Int32>> items_maze;
        private List<Tuple<Position, Int32>> itemsinit_ultimate;

        // Ultimate Gameplay
        private List<Position> agentsinit_ultimate;
        private List<Tuple<Position, Int32>> items_ultimate;
        private Statistics stat_ultimate;

        public GameModel()
        {
            inputfolder = "Assets\\Scripts\\Resource\\"; //"..\\..\\..\\Resource\\";
            outputfolder = "Assets\\Scripts\\Output\\"; //"..\\..\\..\\Output\\";
            config = new Configuration(inputfolder + "fpsbotconfig.xml");
            stat_nav = new Statistics(config.botnum);
            stat_nav2 = new Statistics(config.botnum);
            stat_com = new Statistics(config.botnum);


            rawdata = new List<List<Int32>>();
            map = new List<List<FieldType>>();
            agentsinit_nav = new List<Position>();
            agentsinit_com = new List<Position>();
            items = new List<Tuple<Position, Int32>>();

            arenamap = new List<List<FieldType>>();
            mazemap = new List<List<FieldType>>();
            combatmap = new List<List<FieldType>>();
            ultimatemap = new List<List<FieldType>>();
            agentsinit_arena = new List<Position>();
            agentsinit_maze = new List<Position>();
            agentsinit_combat = new List<Position>();
            items_arena = new List<Tuple<Position, Int32>>();
            items_maze = new List<Tuple<Position, Int32>>();
            itemsinit_ultimate = new List<Tuple<Position, Int32>>();
        }

        public Statistics NavData
        {
            get { return stat_nav; }
            set { }
        }
        public Statistics Nav2Data
        {
            get { return stat_nav2; }
            set { }
        }
        public Statistics ComData
        {
            get { return stat_com; }
            set { }
        }
        public Statistics UltimateData
        {
            get { return stat_ultimate; }
            set { }
        }
        public List<List<FieldType>> Arenamap
        {
            get { return arenamap; }
            set { }
        }
        public List<List<FieldType>> Mazemap
        {
            get { return mazemap; }
            set { }
        }
        public List<List<FieldType>> Combatmap
        {
            get { return combatmap; }
            set { }
        }
        public List<List<FieldType>> Ultimatemap
        {
            get { return ultimatemap; }
            set { }
        }
        public List<Position> Agentsinit_arena
        {
            get { return agentsinit_arena; }
            set { }
        }
        public List<Position> Agentsinit_maze
        {
            get { return agentsinit_maze; }
            set { }
        }
        public List<Position> Agentsinit_combat
        {
            get { return agentsinit_combat; }
            set { }
        }
        public List<Position> Agentsinit_ultimate
        {
            get { return agentsinit_ultimate; }
            set { }
        }
        public Int32 ItemRespawn
        {
            get { return config.tick_itemrespawn; }
            set { }
        }

        public Boolean Initialize_Navigation()
        {
            ReadRawData(config.map_file_nav);
            CreateMap(TrainingSet.navigation);

            if (map == null || map.Count <= 0)
            {
                //Console.WriteLine("Failed to initialize Navigation Controller.");
                return false;
            }

            agents = new Agents(config, ((int)(map.Count / 2) - 1));
            if (!(agents.Initialize_Navigation(agentsinit_nav, config)))
            {
                //Console.WriteLine("Failed to initialize Navigation Controller.");
                return false;
            }

            // Save for replay
            for (int i = 0; i < map.Count; i++)
            {
                List<FieldType> onerow = new List<FieldType>();
                for (int j = 0; j < map[i].Count; j++)
                    onerow.Add(map[i][j]);

                arenamap.Add(onerow);
            }
            for (int i = 0; i < agentsinit_nav.Count; i++)
                agentsinit_arena.Add(new Position(agentsinit_nav[i].X, agentsinit_nav[i].Y));
            for (int i = 0; i < items.Count; i++)
                items_arena.Add(new Tuple<Position, Int32>(new Position(items[i].Item1.X, items[i].Item1.Y), items[i].Item2));

            loopcount = 0;
            return true;
        }

        public Boolean Initialize_Navigation2()
        {
            agentsinit_nav = new List<Position>();
            ReadRawData(config.map_file_nav2);
            CreateMap(TrainingSet.navigation);

            if (map == null || map.Count <= 0)
            {
                //Console.WriteLine("Failed to initialize Navigation Controller.");
                return false;
            }

            agents.ResetOrigin((int)(map.Count / 2) - 1);
            if (!(agents.Initialize_Navigation2(agentsinit_nav, config)))
            {
                //Console.WriteLine("Failed to initialize Navigation Controller.");
                return false;
            }

            // Save for replay
            for (int i = 0; i < map.Count; i++)
            {
                List<FieldType> onerow = new List<FieldType>();
                for (int j = 0; j < map[i].Count; j++)
                    onerow.Add(map[i][j]);

                mazemap.Add(onerow);
            }
            for (int i = 0; i < agentsinit_nav.Count; i++)
                agentsinit_maze.Add(new Position(agentsinit_nav[i].X, agentsinit_nav[i].Y));
            for (int i = 0; i < items.Count; i++)
                items_maze.Add(new Tuple<Position, Int32>(new Position(items[i].Item1.X, items[i].Item1.Y), items[i].Item2));

            loopcount = 0;
            return true;
        }

        public Boolean Initialize_Combat()
        {
            ReadRawData(config.map_file_com);
            CreateMap(TrainingSet.combat);

            if (map == null || map.Count <= 0)
            {
                //Console.WriteLine("Failed to initialize Combat Controller.");
                return false;
            }

            // don't create 'new' agents because we'll need navigation training table for later
            agents.ResetOrigin((int)(map.Count / 2) - 1);
            if (!(agents.Initialize_Combat(agentsinit_com, config)))
            {
                //Console.WriteLine("Failed to initialize Combat Controller.");
                return false;
            }

            // Save for replay
            for (int i = 0; i < map.Count; i++)
            {
                List<FieldType> onerow = new List<FieldType>();
                for (int j = 0; j < map[i].Count; j++)
                    onerow.Add(map[i][j]);

                combatmap.Add(onerow);
            }
            for (int i = 0; i < agentsinit_com.Count; i++)
                agentsinit_combat.Add(new Position(agentsinit_com[i].X, agentsinit_com[i].Y));

            loopcount = 0;
            return true;
        }

        public void ReadRawData(String _map_file)
        {
            // 0     wall  --> 0
            // 1     agent --> 1
            // 2     item  --> 2
            // space empty --> 3

            rawdata = new List<List<Int32>>();
            System.IO.StreamReader file = new System.IO.StreamReader(inputfolder + _map_file);
            String line;

            while ((line = file.ReadLine()) != null)
            {
                List<Int32> currentLine = new List<Int32>();
                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] == '0')
                        currentLine.Add(0);
                    else if (line[i] == '1')
                        currentLine.Add(1);
                    else if (line[i] == '2')
                        currentLine.Add(2);
                    else if (line[i] == ' ')
                        currentLine.Add(3);
                }

                rawdata.Add(currentLine);
            }

            file.Dispose();
        }

        public void CreateMap(TrainingSet trainingset)
        {
            map = new List<List<FieldType>>();
            items = new List<Tuple<Position, Int32>>();

            for (int i = 0; i < rawdata.Count; i++)
            {
                List<FieldType> onerow = new List<FieldType>();
                for (int j = 0; j < rawdata[i].Count; j++)
                {
                    if (rawdata[i][j] == 0)
                        onerow.Add(FieldType.Wall);

                    else if (rawdata[i][j] == 1)
                    {
                        // bot position is double value, not stored in representation of map
                        onerow.Add(FieldType.Empty);
                        if (trainingset == TrainingSet.navigation)
                            agentsinit_nav.Add(new Position(i, j));
                        else if (trainingset == TrainingSet.combat)
                            agentsinit_com.Add(new Position(i, j));
                        else
                            agentsinit_ultimate.Add(new Position(i, j));
                    }
                    else if (rawdata[i][j] == 2)
                    {
                        onerow.Add(FieldType.Item);
                        if (trainingset == TrainingSet.ultimate)
                            items_ultimate.Add(new Tuple<Position, Int32>(new Position(i, j), config.tick_itemrespawn));
                        else
                            items.Add(new Tuple<Position, Int32>(new Position(i, j), config.tick_itemrespawn));
                    }
                    else if (rawdata[i][j] == 3)
                        onerow.Add(FieldType.Empty);
                }

                map.Add(onerow);
            }
        }

        public void ItemManagement(List<List<FieldType>> _map, List<Tuple<Position, Int32>> _items, Position pickedupitem)
        {
            if (pickedupitem.X < 0 || pickedupitem.X >= _map.Count ||
                pickedupitem.Y < 0 || pickedupitem.Y >= _map[0].Count)
                // wtf?
                return;

            Int32 upleft_x = (int)Math.Floor(pickedupitem.X);
            Int32 upleft_y = (int)Math.Floor(pickedupitem.Y);
            Int32 upright_x = (int)Math.Floor(pickedupitem.X);
            Int32 upright_y = (int)Math.Ceiling(pickedupitem.Y);
            Int32 downleft_x = (int)Math.Ceiling(pickedupitem.X);
            Int32 downleft_y = (int)Math.Floor(pickedupitem.Y);
            Int32 downright_x = (int)Math.Ceiling(pickedupitem.X);
            Int32 downright_y = (int)Math.Ceiling(pickedupitem.Y);

            Int32 foundX = 0, foundY = 0;
            if (_map[upleft_x][upleft_y] == FieldType.Item)
            {
                foundX = upleft_x;
                foundY = upleft_y;
            }
            else if (_map[upright_x][upright_y] == FieldType.Item)
            {
                foundX = upright_x;
                foundY = upright_y;
            }
            else if (_map[downleft_x][downleft_y] == FieldType.Item)
            {
                foundX = downleft_x;
                foundY = downleft_y;
            }
            else if (_map[downright_x][downright_y] == FieldType.Item)
            {
                foundX = downright_x;
                foundY = downright_y;
            }
            else
            {
                //Console.WriteLine("item, wtf ?");
                //Console.ReadKey();
                return;
            }

            _map[foundX][foundY] = FieldType.Empty;

            for (int i = 0; i < _items.Count; i++)
            {
                if (foundX == (int)(_items[i].Item1.X) && foundY == (int)(_items[i].Item1.Y))
                {
                    Position tmp_pos = new Position(_items[i].Item1.X, _items[i].Item1.Y);
                    Int32 tmp_countdown = _items[i].Item2 - 1;

                    _items[i] = new Tuple<Position, Int32>(new Position(tmp_pos.X, tmp_pos.Y), tmp_countdown);

                    i = _items.Count;
                }
            }
        }

        public void ItemRespawnCheck(List<List<FieldType>> _map, List<Tuple<Position, Int32>> _items)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Item2 == 0)
                {
                    // Respawn item and reset item tick
                    Position tmp_pos = new Position(_items[i].Item1.X, _items[i].Item1.Y);
                    _items[i] = new Tuple<Position, Int32>(new Position(tmp_pos.X, tmp_pos.Y), config.tick_itemrespawn);
                    _map[(int)(tmp_pos.X)][(int)(tmp_pos.Y)] = FieldType.Item;
                }
                else if (_items[i].Item2 < config.tick_itemrespawn)
                {
                    // Decrease item tick
                    Position tmp_pos = new Position(_items[i].Item1.X, _items[i].Item1.Y);
                    Int32 tmp_countdown = _items[i].Item2 - 1;

                    _items[i] = new Tuple<Position, Int32>(new Position(tmp_pos.X, tmp_pos.Y), tmp_countdown);
                }
            }
        }

        public Boolean TrainNavigationController()
        {
            List<Position> items_pickedup;

            if (loopcount == 0)
            {
                // Initial step

                //view.ShowInit(map, agents.Bots);
                //Console.WriteLine("-- Arena map --");
                //Console.WriteLine("Start Navigation training... ");

                //if (loopcount % 100 == 0) Console.WriteLine("> Iteration " + loopcount);
                agents.Execute_Navigation(map, config, true, stat_nav, loopcount, out items_pickedup);

                // Modify map if items were picked up
                for (int i = 0; i < items_pickedup.Count; i++)
                    ItemManagement(map, items, items_pickedup[i]);

                //view.Show(map, agents.Bots);
                //Console.ForegroundColor = ConsoleColor.Black;

                loopcount++;
            }
            else if (loopcount == config.iterationlimit)
            {
                // End of iterations

                stat_nav.WriteStat_Navigation(outputfolder, "stat_nav.csv", "visual_nav.csv", config.botnum);

                loopcount++;
            }
            else
            {
                // Step in training

                //if (loopcount % 1000 == 0) Console.WriteLine("> Iteration " + loopcount);
                agents.Execute_Navigation(map, config, false, stat_nav, loopcount, out items_pickedup);

                // Modify map if items were picked up
                for (int i = 0; i < items_pickedup.Count; i++)
                    ItemManagement(map, items, items_pickedup[i]);

                // Check items' respawn cycle
                ItemRespawnCheck(map, items);

                //view.Show(map, agents.Bots);
                //Console.ForegroundColor = ConsoleColor.Black;

                loopcount++;
            }

            return loopcount > config.iterationlimit;
        }

        public Boolean TrainNavigationController2()
        {
            List<Position> items_pickedup;

            if (loopcount == 0)
            {
                // Initial step

                //view.ShowInit(map, agents.Bots);
                //Console.WriteLine("-- Maze map -- ");
                //Console.WriteLine("Start Navigation training... ");

                //if (loopcount % 1000 == 0) Console.WriteLine("> Iteration " + loopcount);
                agents.Execute_Navigation(map, config, true, stat_nav2, loopcount, out items_pickedup);

                // Modify map if items were picked up
                for (int i = 0; i < items_pickedup.Count; i++)
                    ItemManagement(map, items, items_pickedup[i]);

                //view.Show(map, agents.Bots);
                //Console.ForegroundColor = ConsoleColor.Black;

                loopcount++;
            }
            else if (loopcount == config.iterationlimit)
            {
                // End of iterations

                stat_nav2.WriteStat_Navigation(outputfolder, "stat_nav2.csv", "visual_nav2.csv", config.botnum);

                loopcount++;
            }
            else
            {
                // Step in training

                //if (loopcount % 1000 == 0) Console.WriteLine("> Iteration " + loopcount);
                agents.Execute_Navigation(map, config, false, stat_nav2, loopcount, out items_pickedup);

                // Modify map if items were picked up
                for (int i = 0; i < items_pickedup.Count; i++)
                    ItemManagement(map, items, items_pickedup[i]);

                // Check items' respawn cycle
                ItemRespawnCheck(map, items);

                //view.Show(map, agents.Bots);
                //Console.ForegroundColor = ConsoleColor.Black;

                loopcount++;
            }

            return loopcount > config.iterationlimit;
        }

        public Boolean TrainCombatController()
        {
            if (loopcount == 0)
            {
                // Initial step

                //view.ShowInit(map, agents.Bots);
                //Console.WriteLine("-- Combat map --");
                //Console.WriteLine("Start Combat training... ");

                //if (loopcount % 1000 == 0) Console.WriteLine("> Iteration " + loopcount);
                agents.Execute_Combat(map, config, true, stat_com, loopcount);

                //view.Show(map, agents.Bots);
                //Console.ForegroundColor = ConsoleColor.Black;

                loopcount++;
            }
            else if (loopcount >= config.iterationlimit)
            {
                // End of iterations

                stat_com.WriteStat_Combat(outputfolder, "stat_com.csv", "visual_com.csv", config.botnum);

                loopcount++;
            }
            else
            {
                // Step in training

                //if (loopcount % 1000 == 0) Console.WriteLine("> Iteration " + loopcount);
                agents.Execute_Combat(map, config, false, stat_com, loopcount);

                agents.CheckAgentRespawn(config);

                //view.Show(map, agents.Bots);
                //Console.ForegroundColor = ConsoleColor.Black;

                loopcount++;
            }

            return loopcount > config.iterationlimit;
        }

        public void TrainAllControllers()
        {
            // Arena map
            Boolean endoftraining = false;
            if (Initialize_Navigation())
            {
                while (!endoftraining)
                {
                    endoftraining = TrainNavigationController();
                }

                //Console.WriteLine("\n Training Navigation Controller ended. (arena)\n");
                //Console.ReadKey();
            }

            // Maze map
            endoftraining = false;
            if (Initialize_Navigation2())
            {
                while (!endoftraining)
                {
                    endoftraining = TrainNavigationController2();
                }

                //Console.WriteLine("\n Training Navigation Controller ended. (maze)\n");
                //Console.ReadKey();
            }

            // Combat map
            endoftraining = false;
            if (Initialize_Combat())
            {
                while (!endoftraining)
                {
                    endoftraining = TrainCombatController();
                }

                //Console.WriteLine("\n Training Combat Controller ended.\n");
                //Console.ReadKey();
            }

            // Initialize with Ultimate Map
            agentsinit_ultimate = new List<Position>();
            items_ultimate = new List<Tuple<Position, Int32>>();
            stat_ultimate = new Statistics(config.botnum);

            ReadRawData(config.map_ultimate);
            CreateMap(TrainingSet.ultimate);

            if (map == null || map.Count <= 0)
            {
                //Console.WriteLine("Failed to initialize map.");
                return;
            }

            agents.ResetOrigin((int)(map.Count / 2) - 1);
            if (!(agents.Initialize_Ultimate(agentsinit_ultimate, config)))
            {
                //Console.WriteLine("Failed to initialize agents.");
                return;
            }

            loopcount = 0;

            // Save for replay
            for (int i = 0; i < map.Count; i++)
            {
                List<FieldType> onerow = new List<FieldType>();
                for (int j = 0; j < map[i].Count; j++)
                    onerow.Add(map[i][j]);

                ultimatemap.Add(onerow);
            }
            //for (int i = 0; i < agentsinit_nav.Count; i++)
            //    agentsinit_arena.Add(new Position(agentsinit_nav[i].X, agentsinit_nav[i].Y));
            for (int i = 0; i < items.Count; i++)
                itemsinit_ultimate.Add(new Tuple<Position, Int32>(new Position(items[i].Item1.X, items[i].Item1.Y), items[i].Item2));

            // Train for Ultimate map
            endoftraining = false;
            while (!endoftraining)
            {
                List<Position> items_pickedup;

                if (loopcount == 0)
                {
                    // Initial step

                    //view.ShowInit(map, agents.Bots);
                    //Console.WriteLine("-- Ultimate map -- ");
                    //Console.WriteLine("Start training... ");

                    //if (loopcount % 1000 == 0) Console.WriteLine("> Iteration " + loopcount);
                    agents.Execute_Ultimate(map, config, stat_ultimate, loopcount, out items_pickedup);

                    // Modify map if items were picked up
                    for (int i = 0; i < items_pickedup.Count; i++)
                        ItemManagement(map, items_ultimate, items_pickedup[i]);

                    loopcount++;
                }
                else if (loopcount == config.iterationlimit)
                {
                    // End of iterations

                    stat_ultimate.WriteStat_Ultimate(outputfolder, "stat_ultimate.csv", "visual_ultimate.csv", "localenv_ultimate.csv", config.botnum);
                    stat_ultimate.Visuals(map, config.mapsize, config.botnum, outputfolder, "heatmaps.csv");
                    loopcount++;
                }
                else
                {
                    // Step in training

                    //if (loopcount % 1000 == 0) Console.WriteLine("> Iteration " + loopcount);
                    agents.Execute_Ultimate(map, config, stat_ultimate, loopcount, out items_pickedup);

                    // Modify map if items were picked up
                    for (int i = 0; i < items_pickedup.Count; i++)
                        ItemManagement(map, items_ultimate, items_pickedup[i]);

                    // Check items' respawn cycle
                    ItemRespawnCheck(map, items_ultimate);

                    agents.CheckAgentRespawn(config);

                    loopcount++;
                }

                endoftraining = loopcount > config.iterationlimit;
            }

            //Console.WriteLine("\n Training for Ultimate map ended.\n");
            //Console.ReadKey();
        }

        //public void ReplayNavigation()
        //{
        //    // Replay learned policy (navigation controller) on the Arena map

        //    Console.WriteLine("-- Arena map --");
        //    Console.WriteLine("Start replay... ");

        //    agents.ResetOrigin((int)(arenamap.Count / 2) - 1);
        //    agents.Initialize_Navigation2(agentsinit_arena, config); // this init function does not create a new sarsa table for navigation
        //    stat_nav = new Statistics(config.botnum);

        //    loopcount = 0;

        //    while (loopcount <= config.iterationlimit)
        //    {
        //        List<Position> items_pickedup;

        //        if (loopcount % 1000 == 0) Console.WriteLine("> Iteration " + loopcount);

        //        agents.Replay_Navigation(arenamap, config, stat_nav, loopcount, out items_pickedup);

        //        for (int i = 0; i < items_pickedup.Count; i++)
        //            ItemManagement(arenamap, items_arena, items_pickedup[i]);

        //        ItemRespawnCheck(arenamap, items_arena);

        //        loopcount++;
        //    }

        //    stat_nav.WriteStat_Navigation(outputfolder, "replay_stat_nav.csv", "replay_visual_nav.csv", config.botnum);

        //    Console.WriteLine("\n Replay (Arena) ended.\n");
        //    Console.ReadKey();
        //}

        //public void ReplayNavigation2()
        //{
        //    // Replay learned policy (navigation controller) on the Maze map

        //    Console.WriteLine("-- Maze map --");
        //    Console.WriteLine("Start replay... ");

        //    agents.ResetOrigin((int)(mazemap.Count / 2) - 1);
        //    agents.Initialize_Navigation2(agentsinit_maze, config); // this init function does not create a new sarsa table for navigation
        //    stat_nav2 = new Statistics(config.botnum);

        //    loopcount = 0;

        //    while (loopcount <= config.iterationlimit)
        //    {
        //        List<Position> items_pickedup;

        //        if (loopcount % 1000 == 0) Console.WriteLine("> Iteration " + loopcount);

        //        agents.Replay_Navigation(mazemap, config, stat_nav2, loopcount, out items_pickedup);

        //        for (int i = 0; i < items_pickedup.Count; i++)
        //            ItemManagement(mazemap, items_maze, items_pickedup[i]);

        //        ItemRespawnCheck(mazemap, items_maze);

        //        loopcount++;
        //    }

        //    stat_nav2.WriteStat_Navigation(outputfolder, "replay_stat_nav2.csv", "replay_visual_nav2.csv", config.botnum);

        //    Console.WriteLine("\n Replay (Maze) ended.\n");
        //    Console.ReadKey();
        //}

        //public void ReplayCombat()
        //{
        //    // Replay learned policy (combat controller) on the Combat map

        //    Console.WriteLine("-- Combat map --");
        //    Console.WriteLine("Start replay... ");

        //    agents.ResetOrigin((int)(combatmap.Count / 2) - 1);
        //    agents.Initialize_Combat2(agentsinit_combat, config); // this init function does not create a new sarsa table for combat
        //    stat_com = new Statistics(config.botnum);

        //    loopcount = 0;

        //    while (loopcount <= config.iterationlimit)
        //    {
        //        if (loopcount % 1000 == 0) Console.WriteLine("> Iteration " + loopcount);

        //        agents.Replay_Combat(map, config, stat_com, loopcount);

        //        agents.CheckAgentRespawn(config);

        //        loopcount++;
        //    }

        //    stat_com.WriteStat_Combat(outputfolder, "replay_stat_com.csv", "replay_visual_com.csv", config.botnum);

        //    Console.WriteLine("\n Replay (Combat) ended.\n");
        //    Console.ReadKey();
        //}
    }
}
