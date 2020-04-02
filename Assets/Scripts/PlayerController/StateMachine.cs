using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class StateMachine
{
    private State _currentState;
    private State _queuedState;
    private Stack<State> _automaton;
    private readonly Dictionary<Type, State> _stateDictionary = new Dictionary<Type, State>();

    public StateMachine(object controller, State[] states)
    {
        foreach (State state in states)
        {
            State instance = UnityEngine.Object.Instantiate(state);
            instance.owner = controller;
            instance.stateMachine = this;
            _stateDictionary.Add(instance.GetType(), instance);
            
            if (_currentState == null)
                _currentState = instance;
        }
        if (_currentState != null)
            _currentState.Enter();
    }

    public void TransitionTo<T>() where T : State
    {
        _queuedState = _stateDictionary[typeof(T)];
    }

    // public void TranasitionBack()
    // {
    //     if (automaton.Count != 0)
    //         queuedState = automaton.Pop();
    // }

    public void Run()
    {
        UpdateState();
        _currentState.Run();
    }

    private void UpdateState()
    {
        if (_queuedState != null &&_queuedState != _currentState)
        {
            if (_currentState != null)
                _currentState.Exit();
            // NOTE(Fors): Pushdown automaton
            // automaton.Push(currentState);
            _currentState = _queuedState;
            _currentState.Enter();
        }
    }

    public String GetState()
    {
        var state = _currentState.GetType().Name;
        return state ?? "";
    }
}