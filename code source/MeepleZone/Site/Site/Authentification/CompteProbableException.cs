using System;

namespace MeepleZone.Site.Authentification;

public sealed class CompteProbableException : Exception
{
    public CompteProbableException() : base("Un compte existe peut-être déjà.")
    {
    }
}
