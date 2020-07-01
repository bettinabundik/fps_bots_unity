using System;
using System.Collections.Generic;

namespace FPSBotsLib
{
    public enum TrainingSet
    {
        navigation,
        combat,
        ultimate
    }

    public enum Action
    {
        turnleft,
        turnright,
        movefw,
        movebw,
        item,
        shoot
    }

    public enum Reward
    {
        collision,
        moving,
        item,
        hit,
        kill,
        miss,
        killed,
        wounded
    }

    public enum Sensor
    {
        none,
        close,
        far
    }

    public enum EnemyDistance
    {
        none,
        close,
        far
    }

    public struct NearestEnemy
    {
        public Boolean visible;
        public EnemyDistance distance;

        public NearestEnemy(Boolean _visible, EnemyDistance _distance)
        {
            visible = _visible;
            if (visible)
                distance = _distance;
            else
                distance = EnemyDistance.none;
        }
    }

    public class State
    {
        // Navigation datafields
        private Sensor wall_center;
        private Sensor wall_left;
        private Sensor wall_right;
        private Sensor item_center;
        private Sensor item_left;
        private Sensor item_right;

        // Combat datafields
        private Boolean cooldownOn;
        private NearestEnemy enemy;
        private Int32 health;

        public State()
        {
            wall_center = Sensor.none;
            wall_left = Sensor.none;
            wall_right = Sensor.none;
            item_center = Sensor.none;
            item_left = Sensor.none;
            item_right = Sensor.none;

            cooldownOn = false;
            enemy = new NearestEnemy(false, EnemyDistance.none);
            health = 100;
        }

        // For navigation
        public State(Sensor w_c, Sensor w_l, Sensor w_r, Sensor i_c, Sensor i_l, Sensor i_r)
        {
            wall_center = w_c;
            wall_left = w_l;
            wall_right = w_r;
            item_center = i_c;
            item_left = i_l;
            item_right = i_r;

            cooldownOn = false;
            enemy = new NearestEnemy(false, EnemyDistance.none);
            health = 100;
        }

        // For combat
        public State(Boolean _cd, Boolean _visibleEnemy, EnemyDistance _enemyDistance, Int32 _health)
        {
            wall_center = Sensor.none;
            wall_left = Sensor.none;
            wall_right = Sensor.none;
            item_center = Sensor.none;
            item_left = Sensor.none;
            item_right = Sensor.none;

            cooldownOn = _cd;
            enemy = new NearestEnemy(_visibleEnemy, _enemyDistance);
            health = _health;
        }

        public State(State _state)
        {
            wall_center = _state.wall_center;
            wall_left = _state.wall_left;
            wall_right = _state.wall_right;
            item_center = _state.item_center;
            item_left = _state.item_left;
            item_right = _state.item_right;
            cooldownOn = _state.cooldownOn;
            enemy = new NearestEnemy(_state.enemy.visible, _state.enemy.distance);
            health = _state.health;
        }

        public Boolean Equal(State state)
        {
            Boolean eq = false;
            Int32 health_threshold = 20;

            if (wall_center == state.wall_center && wall_left == state.wall_left && wall_right == state.wall_right &&
                item_center == state.item_center && item_left == state.item_left && item_right == state.item_right)
            {
                if (cooldownOn == state.cooldownOn &&
                enemy.visible == state.enemy.visible &&
                enemy.distance == state.enemy.distance &&
                Math.Abs(health - state.health) <= health_threshold)
                {
                    eq = true;
                }
            }

            return eq;
        }
    }
    public class SarsaElement
    {
        public State state;

        // Q-values for given state
        public Double q_turnleft;
        public Double q_turnright;
        public Double q_movefw;
        public Double q_movebw;
        public Double q_item;
        public Double q_shoot;

        // Eligibility traces for given state
        public Double et_turnleft;
        public Double et_turnright;
        public Double et_movefw;
        public Double et_movebw;
        public Double et_item;
        public Double et_shoot;

        // Frequency of given state
        public Int32 frequency;

        public SarsaElement(State _state)
        {
            state = new State(_state);
            q_turnleft = 0;
            q_turnright = 0;
            q_movefw = 0;
            q_movebw = 0;
            q_item = 0;
            q_shoot = 0;
            et_turnleft = 0;
            et_turnright = 0;
            et_movefw = 0;
            et_movebw = 0;
            et_item = 0;
            et_shoot = 0;
            frequency = 0;
        }

        public void UpdateValues(Action action, Double qvalue, Double etvalue)
        {
            switch (action)
            {
                case Action.turnleft:
                    q_turnleft = qvalue;
                    et_turnleft = etvalue;
                    break;
                case Action.turnright:
                    q_turnright = qvalue;
                    et_turnright = etvalue;
                    break;
                case Action.movefw:
                    q_movefw = qvalue;
                    et_movefw = etvalue;
                    break;
                case Action.movebw:
                    q_movebw = qvalue;
                    et_movebw = etvalue;
                    break;
                case Action.item:
                    q_item = qvalue;
                    et_item = etvalue;
                    break;
                case Action.shoot:
                    q_shoot = qvalue;
                    et_shoot = etvalue;
                    break;
                default: break;
            }

            frequency++;
        }

        public Double GetQValueForAction(Action action)
        {
            if (action == Action.turnleft)
                return q_turnleft;
            else if (action == Action.turnright)
                return q_turnright;
            else if (action == Action.movefw)
                return q_movefw;
            else if (action == Action.movebw)
                return q_movebw;
            else if (action == Action.item)
                return q_item;
            else if (action == Action.shoot)
                return q_shoot;
            else
                return 0.0;
        }
    }

    public class SarsaTable
    {
        private Double gamma;
        private Double alpha;
        private Double lambda;
        private List<SarsaElement> table;

        private Double alphainit;
        private Double alphatarget;
        private Int32 iterationlimit;

        public SarsaTable(Double _gamma, Double _alphainit, Double _alphatarget, Double _lambda, Int32 _iterationlimit)
        {
            gamma = _gamma;
            alpha = _alphainit;
            lambda = _lambda;
            table = new List<SarsaElement>();

            alphainit = _alphainit;
            alphatarget = _alphatarget;
            iterationlimit = _iterationlimit;
        }

        public Double Gamma
        {
            get { return gamma; }
            set { }
        }

        public List<SarsaElement> Table
        {
            get { return table; }
            set { }
        }

        public Int32 SearchIndex_OrAddElem(SarsaElement elem)
        {
            Int32 index = 0;
            Boolean found = false;

            while (!found && index < table.Count)
            {
                if (table[index].state.Equal(elem.state))
                    found = true;
                else
                    index++;
            }

            if (found)
                return index;
            else
            {
                table.Add(elem);
                return table.Count - 1;
            }
        }

        public void GetQValues_ForReplay(SarsaElement elem,
            out List<Double> q_values, out Boolean randomaction)
        {
            Int32 index = 0;
            Boolean found = false;
            q_values = new List<Double>();

            while (!found && index < table.Count)
            {
                if (table[index].state.Equal(elem.state))
                    found = true;
                else
                    index++;
            }

            if (found)
            {
                randomaction = false;
                q_values.Add(table[index].q_turnleft);
                q_values.Add(table[index].q_turnright);
                q_values.Add(table[index].q_movefw);
                q_values.Add(table[index].q_movebw);
                q_values.Add(table[index].q_item);
                q_values.Add(table[index].q_shoot);
            }
            else
            {
                randomaction = true;
            }
        }

        public void GetBestAction_ForReplay(SarsaElement elem, out Action bestaction, out Double max_q_value, out Boolean randomaction)
        {
            Int32 index = 0;
            Boolean found = false;

            while (!found && index < table.Count)
            {
                if (table[index].state.Equal(elem.state))
                    found = true;
                else
                    index++;
            }

            if (found)
            {
                // Get best Q-value and action according to policy
                max_q_value = table[index].q_turnleft;
                bestaction = Action.turnleft;
                if (table[index].q_turnright > max_q_value)
                {
                    max_q_value = table[index].q_turnright;
                    bestaction = Action.turnright;
                }
                if (table[index].q_movefw > max_q_value)
                {
                    max_q_value = table[index].q_movefw;
                    bestaction = Action.movefw;
                }
                if (table[index].q_movebw > max_q_value)
                {
                    max_q_value = table[index].q_movebw;
                    bestaction = Action.movebw;
                }
                if (table[index].q_item > max_q_value)
                {
                    max_q_value = table[index].q_item;
                    bestaction = Action.item;
                }
                if (table[index].q_shoot > max_q_value)
                {
                    max_q_value = table[index].q_shoot;
                    bestaction = Action.shoot;
                }

                randomaction = false;
            }
            else
            {
                // If state was not found, take a random action
                Random rand = new Random();
                bestaction = (Action)(rand.Next(0, 6));
                max_q_value = 0;
                randomaction = true;
            }
        }

        public Double GetMaxQValue_AllActions(SarsaElement elem)
        {
            Int32 index = SearchIndex_OrAddElem(elem);
            if (index < 0)
                return 0.0; // error

            Double q_max = table[index].q_turnleft;

            if (table[index].q_turnright > q_max)
                q_max = table[index].q_turnright;

            if (table[index].q_movefw > q_max)
                q_max = table[index].q_movefw;

            if (table[index].q_movebw > q_max)
                q_max = table[index].q_movebw;

            if (table[index].q_item > q_max)
                q_max = table[index].q_item;

            if (table[index].q_shoot > q_max)
                q_max = table[index].q_shoot;

            return q_max;
        }

        public void UpdateAll(/*Action nextaction,*/ Double overallreward, Double new_q, Double prev_q)
        {
            Double delta = overallreward + (gamma * new_q) - prev_q;

            foreach (SarsaElement elem in table)
            {
                elem.q_turnleft = elem.q_turnleft + alpha * delta * elem.et_turnleft;
                elem.q_turnright = elem.q_turnright + alpha * delta * elem.et_turnright;
                elem.q_movefw = elem.q_movefw + alpha * delta * elem.et_movefw;
                elem.q_movebw = elem.q_movebw + alpha * delta * elem.et_movebw;
                elem.q_item = elem.q_item + alpha * delta * elem.et_item;
                elem.q_shoot = elem.q_shoot + alpha * delta * elem.et_shoot;

                elem.et_turnleft = gamma * lambda * elem.et_turnleft;
                elem.et_turnright = gamma * lambda * elem.et_turnright;
                elem.et_movefw = gamma * lambda * elem.et_movefw;
                elem.et_movebw = gamma * lambda * elem.et_movebw;
                elem.et_item = gamma * lambda * elem.et_item;
                elem.et_shoot = gamma * lambda * elem.et_shoot;
            }

            // delta = R + gamma * Q(S',A') - Q(S,A)
            // for all S,A:
            //         Q(S,A) <- Q(S,A) + alpha * delta * e(S,A)
            //         e(S,A) <- gamma * lambda * e(S,A)
            //     S := S'
            //     A := A'
        }

        public void DecreaseLearningRate()
        {
            //alpha -= alphainit - (alphatarget / iterationlimit); ???

            alpha -= alphatarget / iterationlimit;

            // the learning rate alpha is LINEARLY decreasing throughout the training with the following equation

            // D            discount rate
            // alpha_i      initial learning rate(= 0.2)
            // alpha_e      target end learning rate(= 0.05)
            // n            total number of iterations

            // D = alpha_i - (alpha_e / n)

            // 0.2 - 0.05 / 5000 = 1.9999
            // alpha = 0.2-- > alpha := 1.9999
            // then alpha := 1.9998...
        }
    }

}
