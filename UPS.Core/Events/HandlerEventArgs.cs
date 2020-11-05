﻿using System;

namespace UPS.Core.Events
{
    public class HandlerEventArgs : EventArgs
    {
        public string Info { get; private set; }
        public HandlerActionType ActionType { get; private set; }
        public HandlerEventArgs(HandlerActionType ActionType)
        {
            this.ActionType = ActionType;
        }
        public HandlerEventArgs(HandlerActionType ActionType, string Info) : this(ActionType)
        {
            this.Info = Info;
        }
    }
}
