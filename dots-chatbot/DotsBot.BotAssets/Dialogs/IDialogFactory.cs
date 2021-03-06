﻿using System.Collections.Generic;

namespace DotsBot.BotAssets.Dialogs
{
    public interface IDialogFactory
    {
        T Create<T>();

        T Create<T, U>(U parameter);

        T Create<T>(IDictionary<string, object> parameters);
    }
}