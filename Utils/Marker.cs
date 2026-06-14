using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Utils;

public static class Marker
{
    public static void Note(this object notes, params object[] additionalNotes) { }
    public static void Reminder(this object notes, params object[] additionalNotes) { }
    public static void Todo(this object notes, params object[] additionalNotes) { }
}
