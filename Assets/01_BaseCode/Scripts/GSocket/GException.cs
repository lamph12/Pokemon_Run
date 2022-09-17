using System;

//===============================================================
//Developer:  CuongCT
//Company:    Rocket Studio
//Date:       2019
//================================================================
public class GException : Exception
{
    public GException(GError error) : base(error.ErrorMessage)
    {
        Error = error;
    }

    public GErrorCode ErrorCode => Error.Error;

    public GError Error { get; }
}