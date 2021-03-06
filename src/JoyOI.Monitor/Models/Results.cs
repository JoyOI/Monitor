﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JoyOI.Monitor.Models
{
    /// <summary>
    /// Judge result.
    /// </summary>
    public enum JudgeResult
    {
        Accepted,
        PresentationError,
        WrongAnswer,
        OutputExceeded,
        TimeExceeded,
        MemoryExceeded,
        RuntimeError,
        CompileError,
        SystemError,
        Hacked,
        Running,
        Pending,
        Hidden
    };

    /// <summary>
    /// Hack result.
    /// </summary>
    public enum HackResult
    {
        Succeeded,
        Failed,
        BadData,
        SystemError,
        Running,
        Pending
    };
}
