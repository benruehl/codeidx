using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeIDX.ViewModels
{
    public enum StatusKind
    {
        Ready,
        Searching,
        Indexing,
        Optimizing,
        Updating,
        Loading,
        Saved,
        Writing
    }
}
