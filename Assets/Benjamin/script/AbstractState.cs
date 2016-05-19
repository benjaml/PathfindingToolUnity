﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace benjamin
{

    public abstract class AbstractState : StateMachine {

        List<ITransition> transitionList = new List<ITransition>();

        public void AddTransition(ITransition trans)
        {
            transitionList.Add(trans);
        }

        public abstract void StateUpdate();
        public abstract void Init();

        public void Check()
        {
            foreach (ITransition trans in transitionList)
            {
                if (trans.Check())
                    AgentLefevre.instance.sm.SetCurrentState(trans.GetNextState());
            }
        }
    }
}
