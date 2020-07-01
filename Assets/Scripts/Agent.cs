using System;
using System.Collections.Generic;

namespace FPSBotsLib
{
    public class Bot
    {
        private TrainingSet trainingset;
        private Int32 id;
        private Int32 health;
        private Double overallreward;
        private Int32 absKills;
        private Int32 absDeaths;
        private Int32 absItems;

        private Position startpos;
        private Position pos;
        private Position prevpos;

        // between 0 and 360
        private Int32 direction;

        // weapon cooldown time
        // 10 ticks is CD time (10 ticks = 10 performed actions)
        private Int32 timeStamp;

        private Int32 origin;

        public Int32 Id
        {
            get { return id; }
            set { }
        }

        public Position Pos
        {
            get { return pos; }
            set { }
        }

        public Int32 Health
        {
            get { return health; }
            set { }
        }

        public Int32 AbsKills
        {
            get { return absKills; }
            set { } // this is modified in Step() if needed
        }

        public Int32 AbsDeaths
        {
            get { return absDeaths; }
            set { } // this is modified in Step() if needed
        }

        public Int32 Direction
        {
            get { return direction; }
            set { }
        }

        public Int32 TimeStamp
        {
            get { return timeStamp; }
            set { }
        }

        public Bot(TrainingSet _set, Int32 _id, Int32 _health, Position _startpos, Int32 _origin)
        {
            trainingset = _set;
            id = _id;
            health = _health;
            overallreward = 0;
            absKills = 0;
            absDeaths = 0;
            absItems = 0;

            startpos = new Position(_startpos.X, _startpos.Y);
            pos = new Position(_startpos.X, _startpos.Y);
            prevpos = new Position(_startpos.X, _startpos.Y);
            direction = 0;
            timeStamp = 0;

            origin = _origin;
        }

        public Boolean Alive()
        {
            return health > 0;
        }

        public Position GetCoordinatePos(Position p)
        {
            // x_rel = y_array - origin.y
            // y_rel = origin.x - x_array

            return new Position(p.Y - origin, origin - p.X);
        }

        public Position GetArrayPos(Position p)
        {
            // x_array = origin.x - y_rel
            // y_array = origin.y + x_rel

            return new Position(origin - p.Y, origin + p.X);
        }

        public void ReduceTimeStamp(Int32 tick_cooldown)
        {
            if (timeStamp <= tick_cooldown && timeStamp > 0)
                timeStamp--;
        }

        public void ResetOrigin(Int32 _origin)
        {
            origin = _origin;
        }

        public void ReduceHealth(Configuration config)
        {
            health += config.enemy_hit;
            if (health < 0)
                health = 0;
        }

        public void Respawn(Configuration config, Int32 _origin)
        {
            // if an agent dies, respawn it in its starting position

            if (startpos == null)
                // wtf?
                return;
            else
            {
                absDeaths++;
                health = config.agent_health;
                pos = new Position(startpos.X, startpos.Y);
                prevpos = new Position(startpos.X, startpos.Y);
                direction = 0;
                timeStamp = 0;

                origin = _origin;

                //Console.WriteLine("Bot " + (id + 1) + " died and respawned at ( " + pos.X + " ; " + pos.Y + " )");
            }
        }

        public void TryStep(Action action, out Int32 nextdirection, out Position nextpos)
        {
            // set tmp values for current values, then modify if needed
            nextdirection = direction;
            nextpos = new Position(pos.X, pos.Y);

            if (action == Action.item || action == Action.shoot)
                return;

            Position coord, nextcoord;
            switch (action)
            {
                case Action.turnleft:
                    // turn LEFT which means INCREASING the angle of direction by 5 degrees
                    //if (direction < 355)
                    //    nextdirection = direction + 5;
                    //else
                    //    nextdirection = 5 - (360 - direction);
                    nextdirection = direction + 5;
                    if (nextdirection >= 360)
                        nextdirection -= 360;
                    break;
                case Action.turnright:
                    // turn RIGHT which means DECREASING the angle of direction by 5 degrees
                    //if (direction >= 5)
                    //    nextdirection = direction - 5;
                    //else
                    //    nextdirection = 360 - (5 - direction);
                    nextdirection = direction - 5;
                    if (nextdirection < 0)
                        nextdirection += 360;
                    break;
                case Action.movefw:
                    // move FORWARDS according to direction (need coordinates instead of array[i,j]
                    // x1 = x + cos(ang) * distance; // distance = 1
                    // y1 = y + sin(ang) * distance;
                    coord = GetCoordinatePos(pos);
                    nextcoord = new Position(
                        coord.X + Math.Cos(DegreetoRadian(direction)),
                        coord.Y + Math.Sin(DegreetoRadian(direction)));
                    nextpos = GetArrayPos(nextcoord);
                    break;
                case Action.movebw:
                    // move BACKWARDS according to direction (need coordinates instead of array[i,j]
                    // x1 = x + cos(ang + 180) * distance; // distance = 1
                    // y1 = y + sin(ang + 180) * distance;
                    coord = GetCoordinatePos(pos);
                    nextcoord = new Position(
                        coord.X + Math.Cos(DegreetoRadian(direction + 180)),
                        coord.Y + Math.Sin(DegreetoRadian(direction + 180)));
                    nextpos = GetArrayPos(nextcoord);
                    break;
                default: break;
            }
        }

        public void TryItem(Action action, List<List<FieldType>> map, Configuration config, out Boolean pickupitem, out Int32 newhealth)
        {
            pickupitem = false;
            newhealth = health;

            if (action != Action.item)
                return;
            else
            {
                pickupitem = CheckCellForFieldType(map, pos, FieldType.Item);
                if (pickupitem)
                {
                    // Heal agent
                    if (health + config.medkit > config.agent_health)
                        newhealth = config.agent_health;
                    else
                        newhealth = health + config.medkit;
                }
            }
        }

        public void TryShoot(Action action, out Boolean canshoot)
        {
            canshoot = false;

            if (action != Action.shoot)
                return;
            else
                canshoot = (timeStamp == 0);
        }

        public void Step(Action nextaction, Position nextpos, Int32 nextdirection, List<Reward> rewards,
            List<List<FieldType>> map, Configuration config, Boolean pickupitem, Boolean canshoot, Int32 newhealth,
            out Boolean collision, out Double distancetravelled)
        {
            //prevpos.X = pos.X;
            //prevpos.Y = pos.Y;
            prevpos = new Position(pos.X, pos.Y);

            if (nextaction == Action.item)
                health = newhealth;
            else if (nextaction == Action.shoot)
                timeStamp = config.tick_cooldown;
            else
            {
                //pos.X = nextpos.X;
                //pos.Y = nextpos.Y;
                pos = new Position(nextpos.X, nextpos.Y);
                direction = nextdirection;
            }

            collision = false;
            distancetravelled = 0;
            for (int i = 0; i < rewards.Count; i++)
            {
                overallreward += GetRewardValue(rewards[i], config);

                if (rewards[i] == Reward.moving)
                    distancetravelled++;
                else if (rewards[i] == Reward.collision)
                    collision = true;
                else if (rewards[i] == Reward.item)
                {
                    absItems++;
                    //Console.WriteLine("Bot " + (id + 1) + " picked up item at ( " + Math.Round(pos.X, 2) + " ; " + Math.Round(pos.Y, 2) + " )");
                }
                else if (rewards[i] == Reward.kill)
                    absKills++;
                //else if (rewards[i] == Reward.killed)
                //   absDeaths++;
            }
        }

        public void RandomAction(List<List<FieldType>> map, Configuration config,
            out Action nextaction, out Int32 nextdirection, out Position nextpos, out Boolean stuck)
        {
            // Take random valid (!) action
            // Make it valid according to training Navigation or Combat
            Random rand = new Random();
            Boolean validnextaction = false;
            nextaction = (Action)(rand.Next(0, 6));
            Boolean pickupitem = false;
            Int32 newhealth = health;
            Boolean canshoot = false;
            nextdirection = direction;
            nextpos = new Position(pos.X, pos.Y);
            stuck = false;
            Int32 stuck_count = 0;

            while (!validnextaction && (stuck_count < 50))
            {
                if (nextaction == Action.item && trainingset == TrainingSet.navigation)
                {
                    TryItem(nextaction, map, config, out pickupitem, out newhealth);
                    if (pickupitem)
                        validnextaction = true;
                }
                else if (nextaction == Action.shoot && trainingset == TrainingSet.combat)
                {
                    TryShoot(nextaction, out canshoot);
                    if (canshoot)
                        validnextaction = true;
                }
                else if (nextaction == Action.movefw || nextaction == Action.movebw || nextaction == Action.turnleft || nextaction == Action.turnright)
                {
                    TryStep(nextaction, out nextdirection, out nextpos);
                    if (!CheckCellForFieldType(map, nextpos, FieldType.Wall))
                        validnextaction = true;
                }

                if (!validnextaction)
                    nextaction = (Action)(rand.Next(0, 6));

                stuck_count++;
            }

            if (stuck_count >= 50)
                stuck = true;
        }

        public void DetectWithSensors(List<List<FieldType>> map, Configuration config,
            out Sensor w_c, out Sensor w_l, out Sensor w_r, out Sensor i_c, out Sensor i_l, out Sensor i_r)
        {
            w_c = Sensor.none;
            w_l = Sensor.none;
            w_r = Sensor.none;
            i_c = Sensor.none;
            i_l = Sensor.none;
            i_r = Sensor.none;

            // None / Close (within 4 meters) / Far (within 10 meters)
            // Collect all item and wall type cells within viewrange (far)

            Int32 tmp = (int)(config.agent_viewrange / 3.0); // = 40 if viewrange = 120
            Int32 viewrange_1 = direction - tmp - (int)(tmp / 2.0); // -60 degrees
            Int32 viewrange_2 = direction - (int)(tmp / 2.0);       // -20
            Int32 viewrange_3 = direction + (int)(tmp / 2.0);       // +20
            Int32 viewrange_4 = direction + tmp + (int)(tmp / 2.0); // +60

            // Right Sensor
            for (int angle = viewrange_1; angle < viewrange_2; angle++)
            {
                Boolean foundwall, founditem;
                Int32 distance;
                Double distance2;
                CheckAngleForWallOrItem(pos, angle, map, out foundwall, out founditem, out distance, out distance2);
                if (foundwall)
                {
                    if (distance < 4)
                        w_r = Sensor.close;
                    else if (distance >= 4 && distance <= 10)
                        w_r = Sensor.far;
                }
                if (founditem)
                {
                    if (distance < 4)
                        i_r = Sensor.close;
                    else if (distance >= 4 && distance <= 10)
                        i_r = Sensor.far;
                }
            }
            // Center Sensor
            for (int angle = viewrange_2; angle < viewrange_3; angle++)
            {
                Boolean foundwall, founditem;
                Int32 distance;
                Double distance2;
                CheckAngleForWallOrItem(pos, angle, map, out foundwall, out founditem, out distance, out distance2);
                if (foundwall)
                {
                    if (distance < 4)
                        w_c = Sensor.close;
                    else if (distance >= 4 && distance <= 10)
                        w_c = Sensor.far;
                }
                if (founditem)
                {
                    if (distance < 4)
                        i_c = Sensor.close;
                    else if (distance >= 4 && distance <= 10)
                        i_c = Sensor.far;
                }
            }
            // Left Sensor
            for (int angle = viewrange_3; angle <= viewrange_4; angle++)
            {
                Boolean foundwall, founditem;
                Int32 distance;
                Double distance2;
                CheckAngleForWallOrItem(pos, angle, map, out foundwall, out founditem, out distance, out distance2);
                if (foundwall)
                {
                    if (distance < 4)
                        w_l = Sensor.close;
                    else if (distance >= 4 && distance <= 10)
                        w_l = Sensor.far;
                }
                if (founditem)
                {
                    if (distance < 4)
                        i_l = Sensor.close;
                    else if (distance >= 4 && distance <= 10)
                        i_l = Sensor.far;
                }
            }
        }

        private Double DegreetoRadian(Double degree)
        {
            return Math.PI * degree / 180.0;
        }

        private Double RadiantoDegree(Double radian)
        {
            return radian * (180.0 / Math.PI);
        }

        private void CheckAngleForWallOrItem(Position position, Int32 angle, List<List<FieldType>> map,
            out Boolean foundwall, out Boolean founditem, out Int32 celldistance, out Double doubledistance)
        {
            // Calculate trajectory for one angle (need coordinates for x, y instead of array[i,j]
            // newx = x + cos(ang) * distance;
            // newy = y + sin(ang) * distance;

            Double cos = Math.Cos(DegreetoRadian(angle));
            Double sin = Math.Sin(DegreetoRadian(angle));
            Int32 dist = 0;
            Boolean outofrange = false;
            foundwall = false;
            founditem = false;
            celldistance = -1;
            doubledistance = -1;
            Position checkpos = new Position(position.X, position.Y);

            while (dist < 10 && !outofrange && !foundwall && !founditem)
            {
                dist++;

                Position coord = GetCoordinatePos(position);
                Position check_coord = new Position(
                    coord.X + cos * dist,
                    coord.Y + sin * dist);
                checkpos = GetArrayPos(check_coord);
                //Position checkpos = new Position(pos.X + cos * dist, pos.Y + sin * dist);
                outofrange = checkpos.X < 0 || checkpos.Y < 0 || checkpos.X > (map.Count - 1) || checkpos.Y > (map[0].Count - 1);
                if (!outofrange)
                {
                    foundwall = CheckCellForFieldType(map, checkpos, FieldType.Wall);
                    if (!foundwall)
                    {
                        founditem = CheckCellForFieldType(map, checkpos, FieldType.Item);
                    }
                }
            }

            if (foundwall || founditem)
            {
                celldistance = dist;
                doubledistance = Math.Sqrt(Math.Pow(position.X - checkpos.X, 2) + Math.Pow(position.Y - checkpos.Y, 2));
            }
        }

        public Int32 ClosestVisibleEnemy(List<List<FieldType>> map, List<Bot> others, Configuration config, out Double closestenemy_dist)
        {
            Position mycoord = GetCoordinatePos(pos);

            List<Tuple<Int32, Double>> visibleenemies = new List<Tuple<Int32, Double>>();
            closestenemy_dist = -1;

            for (int i = 0; i < others.Count; i++)
            {
                if (others[i].Id != id)
                {
                    Position othercoord = GetCoordinatePos(others[i].Pos);
                    Double distance = Math.Sqrt(Math.Pow(othercoord.X - mycoord.X, 2) + Math.Pow(othercoord.Y - mycoord.Y, 2));

                    // Calculate others[i] coordinate position when our agent's position is the temporary origin
                    // x_rel = y_array - tmp_origin.y
                    // y_rel = tmp_origin.x - x_array
                    Position tmp_othercoord = new Position(others[i].Pos.Y - pos.Y, pos.X - others[i].Pos.X);

                    // Get the angle for this temporary point which is relative to the temporary origin
                    // This angle will be between -pi and pi
                    Double angle_rad = Math.Atan2(tmp_othercoord.Y, tmp_othercoord.X);
                    Double angle = RadiantoDegree(angle_rad);

                    // If this angle is between the bot's viewrange (+60, -60).. -> from "left" to "right"..
                    Int32 viewrangehalf = (int)(config.agent_viewrange / 2);
                    Int32 viewrange1 = direction + viewrangehalf;
                    Int32 viewrange2 = direction - viewrangehalf;

                    if (angle <= viewrange1 && angle >= viewrange2)
                    {
                        // if enemy is visible (no walls)..
                        Boolean outofrange = false, hitwall = false, hitbot = false;
                        Double distvector = 0;
                        Double cos_newangle = Math.Cos(angle_rad);
                        Double sin_newangle = Math.Sin(angle_rad);
                        while (!outofrange && !hitwall && !hitbot)
                        {
                            distvector++;

                            Position check_coord = new Position(
                                mycoord.X + cos_newangle * distvector,
                                mycoord.Y + sin_newangle * distvector);
                            Position checkpos = GetArrayPos(check_coord);

                            outofrange = checkpos.X < 0 || checkpos.Y < 0 || checkpos.X > (map.Count - 1) || checkpos.Y > (map[0].Count - 1);

                            if (!outofrange)
                            {
                                hitwall = CheckCellForFieldType(map, checkpos, FieldType.Wall);

                                if (!hitwall)
                                {
                                    Double distfrombotX = Math.Abs(/*checkpos*/ check_coord.X - /*others[i].Pos*/ othercoord.X);
                                    Double distfrombotY = Math.Abs(/*checkpos*/ check_coord.Y - /*others[i].Pos*/ othercoord.Y);
                                    Double threshold = 1.0;
                                    hitbot = distfrombotX <= threshold && distfrombotY <= threshold;
                                }
                            }
                        }

                        if (hitbot)
                            visibleenemies.Add(new Tuple<Int32, Double>(i, distance));
                    }
                }
            }

            if (visibleenemies.Count <= 0)
                return -1;
            else
            {
                Double mindist = visibleenemies[0].Item2;
                Int32 mindist_visible_ID = visibleenemies[0].Item1;
                for (int i = 0; i < visibleenemies.Count; i++)
                {
                    if (mindist > visibleenemies[i].Item2)
                    {
                        mindist = visibleenemies[i].Item2;
                        mindist_visible_ID = visibleenemies[i].Item1;
                    }
                }

                closestenemy_dist = mindist;
                return mindist_visible_ID;
            }
        }

        public void Execute(List<List<FieldType>> map, Double gamma, Configuration config, List<Bot> others, Double allactions_max_q_value,
            out Action nextaction, out Double q_value, out Double etvalue, out Double overallreward, out List<Reward> outrewards,
            out Boolean _collision, out Double _distancetravelled, out Boolean _pickupitem, out Position _nextpos, out Int32 botID_gotshot)
        {
            List<Reward> final_rewards = new List<Reward>();
            Position nextpos = new Position(pos.X, pos.Y);
            Int32 nextdirection = direction;
            nextaction = Action.turnleft;
            q_value = 0;
            etvalue = 1;
            overallreward = 0;
            botID_gotshot = -1;

            Double q_turnleft, q_turnright, q_movefw, q_movebw, q_item, q_shoot;
            Double et_turnleft, et_turnright, et_movefw, et_movebw, et_item, et_shoot;
            List<Reward> r_turnleft, r_turnright, r_movefw, r_movebw, r_item, r_shoot;
            Int32 shot1, shot2, shot3, shot4, shot5;

            // Calculate Q-values / eligibility traces / rewards for each action
            Boolean valid_turnleft = Calc_Q_Value(map, Action.turnleft, others, config, allactions_max_q_value, gamma, out q_turnleft, out et_turnleft, out r_turnleft, out shot1);
            Boolean valid_turnright = Calc_Q_Value(map, Action.turnright, others, config, allactions_max_q_value, gamma, out q_turnright, out et_turnright, out r_turnright, out shot2);
            Boolean valid_movefw = Calc_Q_Value(map, Action.movefw, others, config, allactions_max_q_value, gamma, out q_movefw, out et_movefw, out r_movefw, out shot3);
            Boolean valid_movebw = Calc_Q_Value(map, Action.movebw, others, config, allactions_max_q_value, gamma, out q_movebw, out et_movebw, out r_movebw, out shot4);
            Boolean valid_item = Calc_Q_Value(map, Action.item, others, config, allactions_max_q_value, gamma, out q_item, out et_item, out r_item, out shot5);
            Boolean valid_shoot = Calc_Q_Value(map, Action.shoot, others, config, allactions_max_q_value, gamma, out q_shoot, out et_shoot, out r_shoot, out botID_gotshot);

            // Choose a starting Q-max value to compare later (from valid actions' Q-values)
            if (valid_turnleft)
            {
                q_value = q_turnleft;
                etvalue = et_turnleft;
                nextaction = Action.turnleft;
                final_rewards = new List<Reward>(r_turnleft);
            }
            else if (valid_turnright)
            {
                q_value = q_turnright;
                etvalue = et_turnright;
                nextaction = Action.turnright;
                final_rewards = new List<Reward>(r_turnright);
            }
            else if (valid_movefw)
            {
                q_value = q_movefw;
                etvalue = et_movefw;
                nextaction = Action.movefw;
                final_rewards = new List<Reward>(r_movefw);
            }
            else if (valid_movebw)
            {
                q_value = q_movebw;
                etvalue = et_movebw;
                nextaction = Action.movebw;
                final_rewards = new List<Reward>(r_movebw);
            }
            else if (valid_item)
            {
                q_value = q_item;
                etvalue = et_item;
                nextaction = Action.item;
                final_rewards = new List<Reward>(r_item);
            }
            else if (valid_shoot)
            {
                q_value = q_shoot;
                etvalue = et_shoot;
                nextaction = Action.shoot;
                final_rewards = new List<Reward>(r_shoot);
                // botID_gotshot already has a value
            }

            // Check epsilon greedy
            Random rand = new Random();
            Double epsilonfactor = rand.NextDouble();
            if (config.epsilon < epsilonfactor)
            {
                Boolean stuck;
                RandomAction(map, config, out nextaction, out nextdirection, out nextpos, out stuck);

                switch (nextaction)
                {
                    case Action.turnleft:
                        q_value = q_turnleft;
                        etvalue = et_turnleft;
                        final_rewards = new List<Reward>(r_turnleft);
                        break;
                    case Action.turnright:
                        q_value = q_turnright;
                        etvalue = et_turnright;
                        final_rewards = new List<Reward>(r_turnright);
                        break;
                    case Action.movefw:
                        q_value = q_movefw;
                        etvalue = et_movefw;
                        final_rewards = new List<Reward>(r_movefw);
                        break;
                    case Action.movebw:
                        q_value = q_movebw;
                        etvalue = et_movebw;
                        final_rewards = new List<Reward>(r_movebw);
                        break;
                    case Action.item:
                        q_value = q_item;
                        etvalue = et_item;
                        final_rewards = new List<Reward>(r_item);
                        break;
                    case Action.shoot:
                        q_value = q_shoot;
                        etvalue = et_shoot;
                        final_rewards = new List<Reward>(r_shoot);
                        break;
                    default: break;
                }
            }
            else
            {
                // Choose max Q-value
                if (valid_turnleft && q_value < q_turnleft)
                {
                    nextaction = Action.turnleft;
                    q_value = q_turnleft;
                    etvalue = et_turnleft;
                    final_rewards = new List<Reward>(r_turnleft);
                }

                if (valid_turnright && q_value < q_turnright)
                {
                    nextaction = Action.turnright;
                    q_value = q_turnright;
                    etvalue = et_turnright;
                    final_rewards = new List<Reward>(r_turnright);
                }

                if (valid_movefw && q_value < q_movefw)
                {
                    nextaction = Action.movefw;
                    q_value = q_movefw;
                    etvalue = et_movefw;
                    final_rewards = new List<Reward>(r_movefw);
                }

                if (valid_movebw && q_value < q_movebw)
                {
                    nextaction = Action.movebw;
                    q_value = q_movebw;
                    etvalue = et_movebw;
                    final_rewards = new List<Reward>(r_movebw);
                }

                if (valid_item && q_value < q_item)
                {
                    nextaction = Action.item;
                    q_value = q_item;
                    etvalue = et_item;
                    final_rewards = new List<Reward>(r_item);
                }

                if (valid_shoot && q_value < q_shoot)
                {
                    nextaction = Action.shoot;
                    q_value = q_shoot;
                    etvalue = et_shoot;
                    final_rewards = new List<Reward>(r_shoot);
                }

                // If more than one max Q-value exists, choose randomly
                List<Tuple<Action, List<Reward>>> q_max_values = new List<Tuple<Action, List<Reward>>>();
                if (valid_turnleft && q_turnleft == q_value)
                    q_max_values.Add(new Tuple<Action, List<Reward>>(Action.turnleft, r_turnleft));
                if (valid_turnright && q_turnright == q_value)
                    q_max_values.Add(new Tuple<Action, List<Reward>>(Action.turnright, r_turnright));
                if (valid_movefw && q_movefw == q_value)
                    q_max_values.Add(new Tuple<Action, List<Reward>>(Action.movefw, r_movefw));
                if (valid_movebw && q_movebw == q_value)
                    q_max_values.Add(new Tuple<Action, List<Reward>>(Action.movebw, r_movebw));
                if (valid_item && q_item == q_value)
                    q_max_values.Add(new Tuple<Action, List<Reward>>(Action.item, r_item));
                if (valid_shoot && q_shoot == q_value)
                    q_max_values.Add(new Tuple<Action, List<Reward>>(Action.shoot, r_shoot));

                if (q_max_values.Count > 1)
                {
                    rand = new Random();
                    Int32 rand_index = rand.Next(0, q_max_values.Count);
                    nextaction = q_max_values[rand_index].Item1;
                    final_rewards = new List<Reward>(q_max_values[rand_index].Item2);
                }
            }

            // Step according to Q-max
            Boolean pickupitem = false, canshoot = false;
            Int32 newhealth = health;
            if (nextaction == Action.item)
                TryItem(nextaction, map, config, out pickupitem, out newhealth);
            else if (nextaction == Action.shoot)
                TryShoot(nextaction, out canshoot);
            else
                TryStep(nextaction, out nextdirection, out nextpos);

            Step(nextaction, nextpos, nextdirection, final_rewards, map, config, pickupitem, canshoot, newhealth,
                out _collision, out _distancetravelled);
            _pickupitem = pickupitem;
            _nextpos = new Position(nextpos.X, nextpos.Y);

            ReduceTimeStamp(config.tick_cooldown);

            // Get overall reward as out parameter
            overallreward = 0;
            outrewards = new List<Reward>();
            for (int i = 0; i < final_rewards.Count; i++)
            {
                overallreward += GetRewardValue(final_rewards[i], config);
                outrewards.Add(final_rewards[i]);
            }
        }

        public Boolean Calc_Q_Value(List<List<FieldType>> map, Action action, List<Bot> others, Configuration config, Double allactions_max_q_value,
            Double gamma, out Double q, out Double et, out List<Reward> rewards, out Int32 botID_gotshot)
        {
            q = 0;
            et = 1;
            rewards = new List<Reward>();

            Boolean validaction;
            botID_gotshot = -1;
            rewards = Calc_Rewards(map, action, others, config, out validaction, out botID_gotshot);
            Double overallreward = 0;
            for (int i = 0; i < rewards.Count; i++)
                overallreward += GetRewardValue(rewards[i], config);

            if (validaction)
                // Update function for highest Q-value
                q = overallreward + gamma * allactions_max_q_value;
            else
                q = 0;

            return validaction;
        }

        public List<Reward> Calc_Rewards(List<List<FieldType>> map, Action action, List<Bot> others, Configuration config,
            out Boolean validaction, out Int32 botID_gotshot)
        {
            List<Reward> rewards = new List<Reward>();
            validaction = true;
            botID_gotshot = -1;

            Int32 nextdirection = direction;
            Position nextpos = new Position(pos.X, pos.Y);
            Boolean pickupitem = false, canshoot = false;
            Int32 newhealth = health;
            if (action == Action.item)
                TryItem(action, map, config, out pickupitem, out newhealth);
            else if (action == Action.shoot)
                TryShoot(action, out canshoot);
            else
                TryStep(action, out nextdirection, out nextpos);

            if (CheckCellForFieldType(map, nextpos, FieldType.Wall) ||
                (action == Action.item && !pickupitem) ||
                (action == Action.shoot && !canshoot))
            {
                validaction = false;
                return rewards;
            }

            // Collision
            //if (map[(int)(nextpos.X)][(int)(nextpos.Y)] == FieldType.Wall)
            if (DetectCollision(map, nextpos, config))
                rewards.Add(Reward.collision);

            // Moving
            if (action == Action.movefw || action == Action.movebw)
                rewards.Add(Reward.moving);

            // Item
            if (pickupitem) //(map[(int)(nextpos.X)][(int)(nextpos.Y)] == FieldType.Item)
                rewards.Add(Reward.item);

            if (trainingset == TrainingSet.combat)
            {
                // Hit, Kill or Miss
                if (action == Action.shoot && canshoot)
                {
                    Reward shoot_result = BotShooting(map, nextdirection, nextpos, others, config, out botID_gotshot);
                    if (botID_gotshot >= -1)
                        rewards.Add(shoot_result);
                    // else error
                }

                // Killed or Wounded
                Boolean wounded = false, killed = false;
                BotGettingShot(map, nextpos, others, config, out wounded, out killed);
                if (wounded)
                    rewards.Add(Reward.wounded);
                else if (killed)
                    rewards.Add(Reward.killed);
            }

            return rewards;
        }

        public Double GetRewardValue(Reward reward, Configuration config)
        {
            switch (reward)
            {
                case Reward.collision:
                    return config.rewardnav_collision;
                case Reward.moving:
                    return config.rewardnav_moving;
                case Reward.item:
                    return config.rewardnav_item;
                case Reward.hit:
                    return config.rewardcomb_hit;
                case Reward.kill:
                    return config.rewardcomb_kill;
                case Reward.miss:
                    return config.rewardcomb_miss;
                case Reward.killed:
                    return config.rewardcomb_killed;
                case Reward.wounded:
                    return config.rewardcomb_wounded;
                default:
                    return 0.0;
            }
        }

        public Boolean CheckCellForFieldType(List<List<FieldType>> map, Position position, FieldType fieldtype)
        {
            if (position.X < 0 || position.X >= map.Count || position.Y < 0 || position.Y >= map[0].Count)
                return false;
            else
                return map[(int)(position.X)][(int)(position.Y)] == fieldtype;
        }

        public Boolean DetectCollision(List<List<FieldType>> map, Position position, Configuration config)
        {
            // Check neighbouring cells of agent for walls

            Boolean collision = false;

            Int32 posx = (int)(Math.Floor(position.X));
            Int32 posy = (int)(Math.Floor(position.Y));
            for (int i = posx - 1; i <= posx + 1; i++)
            {
                if (i >= 0 && i < map.Count)
                {
                    for (int j = posy - 1; j <= posy + 1; j++)
                    {
                        if (j >= 0 && j < map[i].Count)
                        {
                            if (map[i][j] == FieldType.Wall)
                            {
                                if (Math.Sqrt(Math.Pow(position.X - i, 2) + Math.Pow(position.Y - j, 2))
                                    < config.collisiontreshold)
                                {
                                    collision = true;
                                }
                            }
                        }
                    }
                }
            }


            //Int32 tmp = (int)(config.agent_viewrange / 3.0); // = 40 if viewrange = 120
            //Int32 viewrange_1 = direction - tmp - (int)(tmp / 2.0); // -60 degrees
            //Int32 viewrange_2 = direction + tmp + (int)(tmp / 2.0); // +60

            //// Right, Center and Left Sensors altogether
            //Int32 angle = viewrange_1;
            //while (angle <= viewrange_2 && !collision)
            //{ 
            //    Boolean foundwall, founditem;
            //    Int32 distance;
            //    Double distance2;
            //    CheckAngleForWallOrItem(position, angle, map, out foundwall, out founditem, out distance, out distance2);
            //    if (foundwall)
            //    {
            //        if (distance2 < config.collisiontreshold)
            //            collision = true;

            //    }
            //    angle++;
            //}

            return collision;
        }

        public Reward BotShooting(List<List<FieldType>> map, Int32 dir, Position pos, List<Bot> others, Configuration config, out Int32 botID_gotshot)
        {
            // Calculate trajectory for shooting action (need coordinates instead of array[i,j]
            // newx = x + cos(ang) * distance;
            // newy = y + sin(ang) * distance;

            Position coord = GetCoordinatePos(pos);
            Double cos_dir = Math.Cos(DegreetoRadian(dir));
            Double sin_dir = Math.Sin(DegreetoRadian(dir));
            Int32 distance = 0;
            Boolean outofrange = false;
            Boolean hitwall = false;
            Boolean hitbot = false;
            botID_gotshot = -1;
            while (!outofrange && !hitwall && !hitbot)
            {
                distance++;

                Position check_coord = new Position(
                                coord.X + cos_dir * distance,
                                coord.Y + sin_dir * distance);
                Position checkpos = GetArrayPos(check_coord);

                outofrange = checkpos.X < 0 || checkpos.Y < 0 || checkpos.X > (map.Count - 1) || checkpos.Y > (map[0].Count - 1);

                if (!outofrange)
                {
                    hitwall = CheckCellForFieldType(map, checkpos, FieldType.Wall);

                    if (!hitwall)
                    {
                        // See if there is any bot in the crossfire
                        // 'distance' is gradually increasing FROM position of bot so we should get the closest other bot in the crossfire
                        Int32 i = 0;
                        Boolean foundbot = false;
                        while (i < others.Count && !foundbot)
                        {
                            if (others[i].Id != id)
                            {
                                Position othercoord = GetCoordinatePos(others[i].Pos);
                                Double distfrombotX = Math.Abs(check_coord.X - othercoord.X);
                                Double distfrombotY = Math.Abs(check_coord.Y - othercoord.Y);
                                Double threshold = 1.0;
                                foundbot = distfrombotX <= threshold && distfrombotY <= threshold;

                                if (foundbot)
                                    botID_gotshot = others[i].Id;
                            }
                            i++;
                        }

                        if (foundbot)
                            hitbot = true;
                    }
                }
            }

            // 'Miss' if there is no bot in the crossfire
            if (outofrange || hitwall)
            {
                botID_gotshot = -1;
                return Reward.miss;
            }
            else if (hitbot)
            {
                // 'Hit' if other bot was shot but its health > 0
                if (others[botID_gotshot].Health + config.enemy_hit > 0)
                    return Reward.hit;
                // 'Kill' if other bot was shot and its health <= 0
                else
                    return Reward.kill;
            }
            else
            {
                // error
                botID_gotshot = -100;
                return Reward.miss;
            }
        }

        public void BotGettingShot(List<List<FieldType>> map, Position pos, List<Bot> others, Configuration config,
            out Boolean wounded, out Boolean killed)
        {
            Position coord = GetCoordinatePos(pos);
            wounded = false;
            killed = false;
            Boolean hitbot = false;

            Int32 b = 0;
            while (b < others.Count && !hitbot)
            {
                if (others[b].Id != id)
                {
                    Double cos_dir = Math.Cos(DegreetoRadian(others[b].Direction));
                    Double sin_dir = Math.Sin(DegreetoRadian(others[b].Direction));
                    Int32 distance = 0;
                    Boolean outofrange = false;
                    Boolean hitwall = false;

                    while (!outofrange && !hitwall && !hitbot)
                    {
                        distance++;

                        Position othercoord = GetCoordinatePos(others[b].Pos);
                        Position check_coord = new Position(
                                othercoord.X + cos_dir * distance,
                                othercoord.Y + sin_dir * distance);
                        Position checkpos = GetArrayPos(check_coord);

                        outofrange = checkpos.X < 0 || checkpos.Y < 0 || checkpos.X > (map.Count - 1) || checkpos.Y > (map[0].Count - 1);

                        if (!outofrange)
                        {
                            hitwall = CheckCellForFieldType(map, checkpos, FieldType.Wall);

                            if (!hitwall)
                            {
                                // See if there is our bot in the crossfire of other bot
                                // 'distance' is gradually increasing FROM position of other bot so we should get the closest bot in the crossfire
                                Double distfrombotX = Math.Abs(check_coord.X - coord.X);
                                Double distfrombotY = Math.Abs(check_coord.Y - coord.Y);
                                Double threshold = 1.0;
                                hitbot = distfrombotX <= threshold && distfrombotY <= threshold;
                            }
                        }
                    }
                }

                b++;
            }

            if (hitbot)
            {
                if (health + config.enemy_hit > 0)
                    wounded = true;
                else
                    killed = true;
            }
        }

        public Double AnalyzeLocalEnv(List<List<FieldType>> map, Configuration config, List<Bot> others)
        {
            Position posInEnv = new Position();

            if (config.localenv < 3 || config.localenv > map.Count || config.localenv % 2 == 0) // has to be odd
                // error
                return 1;

            // Let's get the local environment from the map
            List<List<FieldBehaviour>> area = new List<List<FieldBehaviour>>();
            Int32 tmpx = (int)(pos.X);
            Int32 tmpy = (int)(pos.Y);
            for (int i = tmpx - (config.localenv / 2); i <= tmpx + (config.localenv / 2); i++)
            {
                if (i >= 0 && i < map.Count)
                {
                    List<FieldBehaviour> onerow = new List<FieldBehaviour>();

                    for (int j = tmpy - (config.localenv / 2); j <= tmpy + (config.localenv / 2); j++)
                    {
                        if (j >= 0 && j < map[i].Count)
                        {
                            if (i == tmpx && j == tmpy)
                            {
                                posInEnv.X = area.Count;
                                posInEnv.Y = onerow.Count;
                            }

                            // Check for enemy positions
                            Boolean foundenemy = false;
                            for (int b = 0; b < others.Count; b++)
                            {
                                if (others[b].Id != id)
                                {
                                    if (i == (int)(others[b].Pos.X) && j == (int)(others[b].Pos.Y))
                                    {
                                        foundenemy = true;
                                        onerow.Add(FieldBehaviour.aggressive2);
                                    }
                                }
                            }

                            if (!foundenemy)
                            {
                                // Convert to type for behaviour analysis
                                switch (map[i][j])
                                {
                                    // stealth2 - wall (1)
                                    // stealth1 - item (2)
                                    // aggressive1 - empty (3)
                                    // aggressive2 - enemy (4)

                                    case FieldType.Wall:
                                        onerow.Add(FieldBehaviour.stealth2);
                                        break;
                                    case FieldType.Item:
                                        onerow.Add(FieldBehaviour.stealth1);
                                        break;
                                    case FieldType.Empty:
                                        onerow.Add(FieldBehaviour.aggressive1);
                                        break;
                                    default: break;
                                }
                            }
                        }

                    }

                    area.Add(onerow);
                }
            }

            // Analyze these values
            Int32 tmpvalue = 0;
            Int32 elemcount = 0;
            for (int i = 0; i < area.Count; i++)
            {
                for (int j = 0; j < area[i].Count; j++)
                {
                    switch (area[i][j])
                    {
                        // 1 = stealth2 - wall
                        // 2 = stealth1 - item
                        // 3 = aggressive1 - empty
                        // 4 = aggressive2 - enemy

                        case FieldBehaviour.stealth2:
                            tmpvalue += 1;
                            break;
                        case FieldBehaviour.stealth1:
                            tmpvalue += 2;
                            break;
                        case FieldBehaviour.aggressive1:
                            tmpvalue += 3;
                            break;
                        case FieldBehaviour.aggressive2:
                            tmpvalue += 4;
                            break;
                        default: break;
                    }

                    elemcount++;
                }
            }

            if (tmpvalue == 0 || elemcount == 0)
                return 1;
            else
                return (double)tmpvalue / (double)elemcount;
        }
    }

    public class Agents
    {
        private List<Bot> bots;
        private Int32 botnum;
        private Int32 origin;

        private SarsaTable table_nav;
        private SarsaTable table_comb;

        public List<Bot> Bots
        {
            get { return bots; }
            set { }
        }
        public Agents(Configuration conf, Int32 _origin)
        {
            if (conf == null)
                return;

            botnum = conf.botnum;
            origin = _origin;
        }

        public Boolean Initialize_Navigation(List<Position> _bots, Configuration config)
        {
            // set overall score to zero !!
            botnum = config.botnum;
            bots = new List<Bot>();
            table_nav = new SarsaTable(config.gamma, config.alphainit, config.alphatarget, config.lambda, config.iterationlimit);
            for (int i = 0; i < _bots.Count; i++)
            {
                bots.Add(new Bot(TrainingSet.navigation, i, config.agent_health, new Position(_bots[i].X, _bots[i].Y), origin));
            }

            return botnum == bots.Count;
        }

        public Boolean Initialize_Navigation2(List<Position> _bots, Configuration config)
        {
            botnum = config.botnum;
            bots = new List<Bot>();
            // use same q-table for navigation
            //table_nav = new SarsaTable(config.gamma, config.alphainit, config.alphatarget, config.lambda, config.iterationlimit);
            for (int i = 0; i < _bots.Count; i++)
            {
                bots.Add(new Bot(TrainingSet.navigation, i, config.agent_health, new Position(_bots[i].X, _bots[i].Y), origin));
            }

            return botnum == bots.Count;
        }

        public Boolean Initialize_Combat(List<Position> _bots, Configuration config)
        {
            // set overall score to zero !!
            bots = new List<Bot>();
            table_comb = new SarsaTable(config.gamma, config.alphainit, config.alphatarget, config.lambda, config.iterationlimit);
            for (int i = 0; i < _bots.Count; i++)
            {
                bots.Add(new Bot(TrainingSet.combat, i, config.agent_health, new Position(_bots[i].X, _bots[i].Y), origin));
            }

            return botnum == bots.Count;
        }

        public Boolean Initialize_Ultimate(List<Position> _bots, Configuration config)
        {
            // don't create new sarsa tables
            bots = new List<Bot>();
            for (int i = 0; i < _bots.Count; i++)
            {
                bots.Add(new Bot(TrainingSet.combat, i, config.agent_health, new Position(_bots[i].X, _bots[i].Y), origin));
            }

            return botnum == bots.Count;
        }

        public void ResetOrigin(Int32 _origin)
        {
            origin = _origin;
            for (int i = 0; i < bots.Count; i++)
            {
                bots[i].ResetOrigin(_origin);
            }
        }

        public void Execute_Navigation(List<List<FieldType>> map, Configuration config, Boolean init, Statistics stat, Int32 iteration,
            out List<Position> items_pickedup)
        {
            items_pickedup = new List<Position>();

            // Checking terminal state (end of episodes) is in GameModel)

            // Initial step
            if (init)
            {
                for (int i = 0; i < botnum; i++)
                {
                    // First chosen action is arbitrary for each agent
                    Action nextaction;
                    Int32 nextdirection;
                    Position nextpos;
                    Boolean stuck;
                    bots[i].RandomAction(map, config, out nextaction, out nextdirection, out nextpos, out stuck);

                    // Create new state with navigation datafields only
                    Sensor wall_center, wall_left, wall_right, item_center, item_left, item_right;
                    bots[i].DetectWithSensors(map, config, out wall_center, out wall_left, out wall_right, out item_center, out item_left, out item_right);
                    State newstate = new State(wall_center, wall_left, wall_right, item_center, item_left, item_right);
                    Int32 state_index = table_nav.SearchIndex_OrAddElem(new SarsaElement(newstate));

                    Double init_q_value, init_etvalue;
                    List<Reward> init_rewards;
                    Int32 botID_gotshot;
                    bots[i].Calc_Q_Value(map, nextaction, bots, config, 0, table_nav.Gamma, out init_q_value, out init_etvalue, out init_rewards, out botID_gotshot);

                    table_nav.Table[state_index].UpdateValues(nextaction, init_q_value, init_etvalue);

                    // Step agent
                    Boolean pickupitem = false;
                    Boolean collision = false;
                    Double distancetravelled = 0;
                    Int32 newhealth = bots[i].Health;
                    if (nextaction == Action.item)
                        bots[i].TryItem(nextaction, map, config, out pickupitem, out newhealth);
                    else
                        bots[i].TryStep(nextaction, out nextdirection, out nextpos);

                    bots[i].Step(nextaction, nextpos, nextdirection, init_rewards, map, config, pickupitem, false, newhealth,
                        out collision, out distancetravelled);

                    if (pickupitem)
                        items_pickedup.Add(new Position(nextpos.X, nextpos.Y));

                    // Update statistics
                    stat.UpdateStats(bots[i].Id, collision, distancetravelled, pickupitem, iteration, nextpos, nextaction, 
                        bots[i].Health, bots[i].TimeStamp, init_rewards);
                }
            }

            // One step in SARSA(lambda) training
            else
            {
                for (int i = 0; i < botnum; i++)
                {
                    if (bots[i].Alive())
                    {
                        Sensor wall_center, wall_left, wall_right, item_center, item_left, item_right;
                        bots[i].DetectWithSensors(map, config, out wall_center, out wall_left, out wall_right, out item_center, out item_left, out item_right);
                        State newstate = new State(wall_center, wall_left, wall_right, item_center, item_left, item_right);
                        Double allactions_max_q_value = table_nav.GetMaxQValue_AllActions(new SarsaElement(newstate));

                        // Calculate new Q-value and Eligibility trace
                        Action nextaction;
                        Double new_q_value, new_etvalue;
                        Double overallreward;
                        List<Reward> rewards;
                        Boolean collision, pickupitem;
                        Double distancetravelled;
                        Position nextpos;
                        Int32 botID_gotshot;
                        bots[i].Execute(map, table_nav.Gamma, config, bots, allactions_max_q_value,
                            out nextaction, out new_q_value, out new_etvalue, out overallreward, out rewards, 
                            out collision, out distancetravelled, out pickupitem, out nextpos, out botID_gotshot);

                        // Update Sarsa-table at (state, action) with new Q-value and ET
                        // new_etvalue should be 1 at this point
                        Int32 state_index = table_nav.SearchIndex_OrAddElem(new SarsaElement(newstate));
                        Double prev_q_value = table_nav.Table[state_index].GetQValueForAction(nextaction);
                        table_nav.Table[state_index].UpdateValues(nextaction, new_q_value, new_etvalue);
                        // Update Sarsa-table at all (state, action) pairs with new Q-value and ET
                        table_nav.UpdateAll(overallreward, new_q_value, prev_q_value);

                        if (nextaction == Action.item)
                            items_pickedup.Add(new Position(bots[i].Pos.X, bots[i].Pos.Y));

                        // Update statistics
                        stat.UpdateStats(bots[i].Id, collision, distancetravelled, pickupitem, iteration, nextpos, nextaction, 
                            bots[i].Health, bots[i].TimeStamp, rewards);
                    }
                }
            }

            // Decrease learning rate of SARSA
            table_nav.DecreaseLearningRate();
        }

        public void Execute_Combat(List<List<FieldType>> map, Configuration config, Boolean init, Statistics stat, Int32 iteration)
        {
            // Checking terminal state (end of episodes) is in GameModel)

            // Initial step
            if (init)
            {
                for (int i = 0; i < botnum; i++)
                {
                    // First chosen action is arbitrary for each agent
                    Action nextaction;
                    Int32 nextdirection;
                    Position nextpos;
                    Boolean stuck;
                    bots[i].RandomAction(map, config, out nextaction, out nextdirection, out nextpos, out stuck);

                    Double closestenemy_dist;
                    Int32 closestvisibleID = bots[i].ClosestVisibleEnemy(map, bots, config, out closestenemy_dist);
                    Boolean visible_exists = closestvisibleID >= 0 && closestvisibleID < bots.Count;
                    EnemyDistance distance_type = EnemyDistance.none;
                    if (visible_exists)
                    {
                        if (closestenemy_dist <= config.enemy_close)
                            distance_type = EnemyDistance.close;
                        else if (closestenemy_dist <= config.enemy_far)
                            distance_type = EnemyDistance.far;
                    }
                    State newstate = new State(false, visible_exists, distance_type, bots[i].Health);
                    Int32 state_index = table_comb.SearchIndex_OrAddElem(new SarsaElement(newstate));

                    Double init_q_value, init_etvalue;
                    List<Reward> init_rewards;
                    Int32 botID_gotshot;
                    bots[i].Calc_Q_Value(map, nextaction, bots, config, 0, table_comb.Gamma, out init_q_value, out init_etvalue, out init_rewards, out botID_gotshot);

                    table_comb.Table[state_index].UpdateValues(nextaction, init_q_value, init_etvalue);

                    // Step agent
                    Boolean canshoot = false;
                    Boolean collision;
                    Double distancetravelled;
                    if (nextaction == Action.shoot)
                        bots[i].TryShoot(nextaction, out canshoot);
                    else
                        bots[i].TryStep(nextaction, out nextdirection, out nextpos);

                    bots[i].Step(nextaction, nextpos, nextdirection, init_rewards, map, config, false, canshoot, bots[i].Health,
                        out collision, out distancetravelled);

                    bots[i].ReduceTimeStamp(config.tick_cooldown);

                    // Update statistics
                    stat.UpdateStats(bots[i].Id, collision, distancetravelled, false, iteration, nextpos, nextaction, 
                        bots[i].Health, bots[i].TimeStamp, init_rewards);
                    stat.UpdateKD(bots[i].Id, bots[i].AbsKills, bots[i].AbsDeaths);

                    // Check if another bot got shot
                    if (nextaction == Action.shoot && botID_gotshot >= 0 && botID_gotshot < bots.Count)
                    {
                        bots[botID_gotshot].ReduceHealth(config);
                        stat.UpdateHitMiss(i, Reward.hit);
                    }
                    else if (nextaction == Action.shoot && botID_gotshot < 0 || botID_gotshot >= bots.Count)
                        stat.UpdateHitMiss(i, Reward.miss);
                }
            }

            // One step in SARSA(lambda) training
            else
            {
                for (int i = 0; i < botnum; i++)
                {
                    if (bots[i].Alive())
                    {
                        Double closestenemy_dist;
                        Int32 closestvisibleID = bots[i].ClosestVisibleEnemy(map, bots, config, out closestenemy_dist);
                        Boolean visible_exists = closestvisibleID >= 0 && closestvisibleID < bots.Count;
                        EnemyDistance distance_type = EnemyDistance.none;
                        if (visible_exists)
                        {
                            if (closestenemy_dist <= config.enemy_close)
                                distance_type = EnemyDistance.close;
                            else if (closestenemy_dist <= config.enemy_far)
                                distance_type = EnemyDistance.far;
                        }
                        State newstate = new State(bots[i].TimeStamp > 0, visible_exists, distance_type, bots[i].Health);
                        Double allactions_max_q_value = table_comb.GetMaxQValue_AllActions(new SarsaElement(newstate));

                        // Calculate new Q-value and Eligibility trace
                        Action nextaction;
                        Double new_q_value, new_etvalue;
                        Double overallreward;
                        List<Reward> rewards;
                        Boolean collision, pickupitem;
                        Double distancetravelled;
                        Position nextpos;
                        Int32 botID_gotshot;
                        bots[i].Execute(map, table_comb.Gamma, config, bots, allactions_max_q_value,
                            out nextaction, out new_q_value, out new_etvalue, out overallreward, out rewards,
                            out collision, out distancetravelled, out pickupitem, out nextpos, out botID_gotshot);

                        // Update Sarsa-table at (state, action) with new Q-value and ET
                        // new_etvalue should be 1 at this point
                        Int32 state_index = table_comb.SearchIndex_OrAddElem(new SarsaElement(newstate));
                        Double prev_q_value = table_comb.Table[state_index].GetQValueForAction(nextaction);
                        table_comb.Table[state_index].UpdateValues(nextaction, new_q_value, new_etvalue);
                        // Update Sarsa-table at all (state, action) pairs with new Q-value and ET
                        table_comb.UpdateAll(overallreward, new_q_value, prev_q_value);

                        // Update statistics
                        stat.UpdateStats(bots[i].Id, collision, distancetravelled, false, iteration, nextpos, nextaction, 
                            bots[i].Health, bots[i].TimeStamp, rewards);
                        stat.UpdateKD(bots[i].Id, bots[i].AbsKills, bots[i].AbsDeaths);

                        // Check if another bot got shot
                        if (nextaction == Action.shoot && botID_gotshot >= 0 && botID_gotshot < bots.Count)
                        {
                            //Console.WriteLine("Bot " + (bots[i].Id + 1) + " - shot " + (botID_gotshot + 1));
                            bots[botID_gotshot].ReduceHealth(config);
                            stat.UpdateHitMiss(i, Reward.hit);
                        }
                        else if (nextaction == Action.shoot && botID_gotshot < 0 || botID_gotshot >= bots.Count)
                            stat.UpdateHitMiss(i, Reward.miss);
                    }
                }
            }

            // Decrease learning rate of SARSA
            table_comb.DecreaseLearningRate();
        }

        public void Execute_Ultimate(List<List<FieldType>> map, Configuration config, Statistics stat, Int32 iteration,
            out List<Position> items_pickedup)
        {
            items_pickedup = new List<Position>();

            // One step in SARSA(lambda) training
            // With new method
            // 1. analyze local environment
            // 2. choose stealth / aggressive based on this value
            // 3. step according to value
            //      stealth    <=> navigation
            //      aggressive <=> combat

            for (int i = 0; i < botnum; i++)
            {
                if (bots[i].Alive())
                {
                    State newstate;
                    Double allactions_max_q_value;

                    Double localenv_value = bots[i].AnalyzeLocalEnv(map, config, bots);

                    // Stealth for: 1 <= value < 2.5
                    if (localenv_value >= 1 && localenv_value < 2.5)
                    {
                        Sensor wall_center, wall_left, wall_right, item_center, item_left, item_right;
                        bots[i].DetectWithSensors(map, config, out wall_center, out wall_left, out wall_right, out item_center, out item_left, out item_right);
                        newstate = new State(wall_center, wall_left, wall_right, item_center, item_left, item_right);
                        allactions_max_q_value = table_nav.GetMaxQValue_AllActions(new SarsaElement(newstate));
                    }
                    // Aggressive for: 2.5 <= value <= 4
                    else if (localenv_value >= 2.5 && localenv_value <= 4)
                    {
                        Double closestenemy_dist;
                        Int32 closestvisibleID = bots[i].ClosestVisibleEnemy(map, bots, config, out closestenemy_dist);
                        Boolean visible_exists = closestvisibleID >= 0 && closestvisibleID < bots.Count;
                        EnemyDistance distance_type = EnemyDistance.none;
                        if (visible_exists)
                        {
                            if (closestenemy_dist <= config.enemy_close)
                                distance_type = EnemyDistance.close;
                            else if (closestenemy_dist <= config.enemy_far)
                                distance_type = EnemyDistance.far;
                        }
                        newstate = new State(bots[i].TimeStamp > 0, visible_exists, distance_type, bots[i].Health);
                        allactions_max_q_value = table_comb.GetMaxQValue_AllActions(new SarsaElement(newstate));
                    }
                    else
                        // error
                        return;

                    // Calculate Q-value and Eligibility trace
                    Action nextaction;
                    Double new_q_value, new_etvalue;
                    Double overallreward;
                    List<Reward> rewards;
                    Boolean collision, pickupitem;
                    Double distancetravelled;
                    Position nextpos;
                    Int32 botID_gotshot;
                    bots[i].Execute(map, table_nav.Gamma, config, bots, allactions_max_q_value,
                        out nextaction, out new_q_value, out new_etvalue, out overallreward, out rewards,
                        out collision, out distancetravelled, out pickupitem, out nextpos, out botID_gotshot);

                    // Update Sarsa-table at (state, action) with new Q-value and ET
                    // from here --
                    Int32 state_index;
                    Double prev_q_value;
                    if (localenv_value >= 1 && localenv_value < 2.5)
                    {
                        state_index = table_nav.SearchIndex_OrAddElem(new SarsaElement(newstate));
                        prev_q_value = table_nav.Table[state_index].GetQValueForAction(nextaction);
                        table_nav.Table[state_index].UpdateValues(nextaction, new_q_value, new_etvalue);
                        table_nav.UpdateAll(overallreward, new_q_value, prev_q_value);
                    }
                    else if (localenv_value >= 2.5 && localenv_value <= 4)
                    {
                        state_index = table_comb.SearchIndex_OrAddElem(new SarsaElement(newstate));
                        prev_q_value = table_comb.Table[state_index].GetQValueForAction(nextaction);
                        table_comb.Table[state_index].UpdateValues(nextaction, new_q_value, new_etvalue);
                        table_comb.UpdateAll(overallreward, new_q_value, prev_q_value);
                    }
                    // -- until here: comment out to exclude training

                    if (nextaction == Action.item)
                        items_pickedup.Add(new Position(bots[i].Pos.X, bots[i].Pos.Y));

                    if (nextaction == Action.shoot && botID_gotshot >= 0 && botID_gotshot < bots.Count)
                    {
                        //Console.WriteLine("Bot " + (bots[i].Id + 1) + " - shot " + (botID_gotshot + 1));
                        bots[botID_gotshot].ReduceHealth(config);
                        stat.UpdateHitMiss(i, Reward.hit);
                    }
                    else if (nextaction == Action.shoot && botID_gotshot < 0 || botID_gotshot >= bots.Count)
                        stat.UpdateHitMiss(i, Reward.miss);

                    stat.UpdateStats(bots[i].Id, collision, distancetravelled, pickupitem, iteration, nextpos, nextaction, 
                        bots[i].Health, bots[i].TimeStamp, rewards);
                    stat.UpdateKD(bots[i].Id, bots[i].AbsKills, bots[i].AbsDeaths);
                    stat.UpdateLocalEnvValues(bots[i].Id, localenv_value);
                }
            }

            // comment these too if training is off
            table_nav.DecreaseLearningRate();
            table_comb.DecreaseLearningRate();
        }

        public void CheckAgentRespawn(Configuration config)
        {
            for (int i = 0; i < bots.Count; i++)
            {
                if (!(bots[i].Alive()))
                {
                    bots[i].Respawn(config, origin);
                }
            }
        }

        // Sarsa(lambda) with eligibility traces
        // Q(S,A) = 0, e(S,A) = 0 for all S,A initially
        // initialize S
        // choose A from S using policy derived from Q (epsilon greedy + highest Q-value)
        // for each update step t
        //     take action A, observe R, S' (reward, next state)
        //     choose A' from S' using policy derived from Q (epsilon greedy + highest Q-value)
        //     e(S,A) = 1
        //     for all S,A:
        //         Q(S,A) <- Q(S,A) + alpha * ( R + gamma * Q(S',A') - Q(S,A) ) * e(S,A)
        //         e(S,A) <- gamma * lambda * e(S,A)
        //     S := S'
        //     A := A'
        // until S is terminal



        public void Replay_Navigation(List<List<FieldType>> map, Configuration config, Statistics stat, Int32 iteration,
            out List<Position> items_pickedup)
        {
            items_pickedup = new List<Position>();

            for (int i = 0; i < botnum; i++)
            {
                if (bots[i].Alive())
                {
                    Sensor wall_center, wall_left, wall_right, item_center, item_left, item_right;
                    bots[i].DetectWithSensors(map, config, out wall_center, out wall_left, out wall_right, out item_center, out item_left, out item_right);
                    State newstate = new State(wall_center, wall_left, wall_right, item_center, item_left, item_right);

                    // Search for state in existing Sarsa-table
                    //List<Double> q_values;
                    //Boolean randomaction;
                    //table_nav.GetQValues_ForReplay(new SarsaElement(newstate), out q_values, out randomaction);

                    // Search for best action (highest Q-value) among valid actions
                    //Action nextaction;
                    //Int32 nextdirection = bots[i].Direction;
                    //Position nextpos = new Position(bots[i].Pos.X, bots[i].Pos.Y);
                    //Double max_q = -100;
                    //List<Reward> rewards;
                    //Boolean validaction;
                    //Int32 botID_gotshot;
                    //if (randomaction)
                    //    bots[i].RandomAction(map, config, out nextaction, out nextdirection, out nextpos);
                    //else
                    //{
                    //    List<Tuple<Double, Boolean>> valid_q_values = new List<Tuple<Double, Boolean>>();
                    //    rewards = bots[i].Calc_Rewards(map, Action.turnleft, bots, config, out validaction, out botID_gotshot);
                    //    valid_q_values.Add(new Tuple<Double, Boolean>(q_values[0], validaction));

                    //    rewards = bots[i].Calc_Rewards(map, Action.turnright, bots, config, out validaction, out botID_gotshot);
                    //    valid_q_values.Add(new Tuple<Double, Boolean>(q_values[1], validaction));

                    //    rewards = bots[i].Calc_Rewards(map, Action.movefw, bots, config, out validaction, out botID_gotshot);
                    //    valid_q_values.Add(new Tuple<Double, Boolean>(q_values[2], validaction));

                    //    rewards = bots[i].Calc_Rewards(map, Action.movebw, bots, config, out validaction, out botID_gotshot);
                    //    valid_q_values.Add(new Tuple<Double, Boolean>(q_values[3], validaction));

                    //    rewards = bots[i].Calc_Rewards(map, Action.item, bots, config, out validaction, out botID_gotshot);
                    //    valid_q_values.Add(new Tuple<Double, Boolean>(q_values[4], validaction));

                    //    nextaction = Action.turnleft;
                    //    max_q = valid_q_values[0].Item1;

                    //    if (valid_q_values[1].Item2 && valid_q_values[1].Item1 > max_q)
                    //    {
                    //        nextaction = Action.turnright;
                    //        max_q = valid_q_values[1].Item1;
                    //    }
                    //    if (valid_q_values[2].Item2 && valid_q_values[2].Item1 > max_q)
                    //    {
                    //        nextaction = Action.movefw;
                    //        max_q = valid_q_values[2].Item1;
                    //    }
                    //    if (valid_q_values[3].Item2 && valid_q_values[3].Item1 > max_q)
                    //    {
                    //        nextaction = Action.movebw;
                    //        max_q = valid_q_values[3].Item1;
                    //    }
                    //    if (valid_q_values[4].Item2 && valid_q_values[4].Item1 > max_q)
                    //    {
                    //        nextaction = Action.item;
                    //        max_q = valid_q_values[4].Item1;
                    //    }
                    //    there's no shooting here
                    //}


                    // Search for state in existing Sarsa-table, get the best action of this state
                    Boolean randomaction;
                    Action nextaction;
                    Double q_value;
                    Boolean pickupitem = false;
                    table_nav.GetBestAction_ForReplay(new SarsaElement(newstate), out nextaction, out q_value, out randomaction);

                    if (nextaction == Action.item)
                        pickupitem = bots[i].CheckCellForFieldType(map, bots[i].Pos, FieldType.Item);

                    // Check if it's valid action
                    Int32 nextdirection = bots[i].Direction;
                    Position nextpos = new Position(bots[i].Pos.X, bots[i].Pos.Y);
                    Boolean stuck = false;
                    if (randomaction || (nextaction == Action.item && !pickupitem))
                        bots[i].RandomAction(map, config, out nextaction, out nextdirection, out nextpos, out stuck);

                    if (stuck)
                    {
                        bots[i].Respawn(config, origin);
                        //Console.WriteLine(" --- Stuck");
                        //Console.ReadKey();
                        return;
                    }
                    //if (randomaction)
                    //    Console.WriteLine(bots[i].Id + ": " + nextaction + " (random)");
                    //else
                    //    Console.WriteLine(bots[i].Id + ": " + nextaction);

                    // Step agent
                    Boolean collision = false;
                    Double distancetravelled = 0;
                    Int32 newhealth = bots[i].Health;
                    Boolean validaction;
                    Int32 botID_gotshot = -1;
                    List<Reward> final_rewards = bots[i].Calc_Rewards(map, nextaction, bots, config, out validaction, out botID_gotshot);


                    //Console.WriteLine("   Q-value " + q_value);

                    if (nextaction == Action.item)
                        bots[i].TryItem(nextaction, map, config, out pickupitem, out newhealth);
                    else
                        bots[i].TryStep(nextaction, out nextdirection, out nextpos);

                    bots[i].Step(nextaction, nextpos, nextdirection, final_rewards, map, config, pickupitem, false, newhealth,
                        out collision, out distancetravelled);

                    //Console.WriteLine("   Pos " + Math.Round(bots[i].Pos.X, 2) + ";" + Math.Round(bots[i].Pos.Y, 2));
                    //Console.WriteLine("   Dir " + bots[i].Direction);

                    if (pickupitem)
                        items_pickedup.Add(new Position(nextpos.X, nextpos.Y));

                    // Update statistics
                    stat.UpdateStats(bots[i].Id, collision, distancetravelled, pickupitem, iteration, nextpos, nextaction, 
                        bots[i].Health, bots[i].TimeStamp, final_rewards);
                }
            }

            //Console.ReadKey();
            //Console.WriteLine();
        }

        public void Replay_Combat(List<List<FieldType>> map, Configuration config, Statistics stat, Int32 iteration)
        {
            for (int i = 0; i < botnum; i++)
            {
                if (bots[i].Alive())
                {
                    Double closestenemy_dist;
                    Int32 closestvisibleID = bots[i].ClosestVisibleEnemy(map, bots, config, out closestenemy_dist);
                    Boolean visible_exists = closestvisibleID >= 0 && closestvisibleID < bots.Count;
                    EnemyDistance distance_type = EnemyDistance.none;
                    if (visible_exists)
                    {
                        if (closestenemy_dist <= config.enemy_close)
                            distance_type = EnemyDistance.close;
                        else if (closestenemy_dist <= config.enemy_far)
                            distance_type = EnemyDistance.far;
                    }
                    State newstate = new State(false, visible_exists, distance_type, bots[i].Health);

                    // Search for state in existing Sarsa-table, get the best action of this state
                    Boolean randomaction;
                    Action nextaction;
                    Double max_q_value;
                    table_nav.GetBestAction_ForReplay(new SarsaElement(newstate), out nextaction, out max_q_value, out randomaction);

                    // Check if it's valid action
                    Boolean canshoot = (bots[i].TimeStamp == 0);
                    Int32 nextdirection = bots[i].Direction;
                    Position nextpos = new Position(bots[i].Pos.X, bots[i].Pos.Y);
                    Boolean stuck = false;
                    if (randomaction || (nextaction == Action.shoot && !canshoot))
                        bots[i].RandomAction(map, config, out nextaction, out nextdirection, out nextpos, out stuck);

                    if (stuck)
                    {
                        bots[i].Respawn(config, origin);
                        //Console.WriteLine(" --- Stuck");
                        //Console.ReadKey();
                        return;
                    }

                    // Step agent
                    Boolean collision = false;
                    Double distancetravelled = 0;
                    Int32 newhealth = bots[i].Health;
                    Boolean validaction;
                    Int32 botID_gotshot = -1;
                    List<Reward> final_rewards = bots[i].Calc_Rewards(map, nextaction, bots, config, out validaction, out botID_gotshot);

                    if (nextaction == Action.shoot)
                        bots[i].TryShoot(nextaction, out canshoot);
                    else
                        bots[i].TryStep(nextaction, out nextdirection, out nextpos);

                    bots[i].Step(nextaction, nextpos, nextdirection, final_rewards, map, config, false, canshoot, bots[i].Health,
                        out collision, out distancetravelled);

                    bots[i].ReduceTimeStamp(config.tick_cooldown);

                    // Update statistics
                    stat.UpdateStats(bots[i].Id, collision, distancetravelled, false, iteration, nextpos, nextaction, 
                        bots[i].Health, bots[i].TimeStamp, final_rewards);
                    stat.UpdateKD(bots[i].Id, bots[i].AbsKills, bots[i].AbsDeaths);

                    // Check if another bot got shot
                    if (nextaction == Action.shoot && botID_gotshot >= 0 && botID_gotshot < bots.Count)
                    {
                        bots[botID_gotshot].ReduceHealth(config);
                    }
                }
            }
        }
    }
}
