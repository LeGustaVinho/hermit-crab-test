using System;
using System.Collections.Generic;

namespace HermitCrab.Core
{
    /// <summary>
    /// Defines the possible statuses a Behavior Tree node can return.
    /// </summary>
    public enum BTStatus
    {
        Success,
        Failure,
        Running
    }

    /// <summary>
    /// Abstract base class for a Behavior Tree node.
    /// </summary>
    public abstract class BTNode
    {
        /// <summary>
        /// Executes the behavior of the node.
        /// </summary>
        /// <returns>A BTStatus indicating the result of execution.</returns>
        public abstract BTStatus Execute();
    }

    /// <summary>
    /// A stateless sequence node that executes its children in order.
    /// Returns Failure immediately if any child fails; returns Success if all succeed.
    /// </summary>
    public class StatelessBTSequence : BTNode
    {
        private readonly List<BTNode> children;

        public StatelessBTSequence(List<BTNode> children)
        {
            this.children = children;
        }

        public override BTStatus Execute()
        {
            foreach (BTNode child in children)
            {
                BTStatus status = child.Execute();
                if (status != BTStatus.Success)
                {
                    return status;
                }
            }
            return BTStatus.Success;
        }
    }

    /// <summary>
    /// A stateless selector node that executes its children in order.
    /// Returns Success if any child succeeds; if all fail, returns Failure.
    /// </summary>
    public class StatelessBTSelector : BTNode
    {
        private readonly List<BTNode> children;

        public StatelessBTSelector(List<BTNode> children)
        {
            this.children = children;
        }

        public override BTStatus Execute()
        {
            foreach (BTNode child in children)
            {
                BTStatus status = child.Execute();
                if (status == BTStatus.Success)
                {
                    return BTStatus.Success;
                }
            }
            return BTStatus.Failure;
        }
    }

    /// <summary>
    /// A condition node that evaluates a predicate function.
    /// Returns Success if the condition is true, Failure otherwise.
    /// </summary>
    public class BTCondition : BTNode
    {
        private readonly Func<bool> condition;

        public BTCondition(Func<bool> condition)
        {
            this.condition = condition;
        }

        public override BTStatus Execute()
        {
            return condition() ? BTStatus.Success : BTStatus.Failure;
        }
    }

    /// <summary>
    /// An action node that executes a function and returns its BTStatus.
    /// </summary>
    public class BTAction : BTNode
    {
        private readonly Func<BTStatus> action;

        public BTAction(Func<BTStatus> action)
        {
            this.action = action;
        }

        public override BTStatus Execute()
        {
            return action();
        }
    }
}