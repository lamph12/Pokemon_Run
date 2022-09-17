using System;
using UniRx;

public class GObservable
{
    public static Subject<ProfileModel> GLoginObservable = new Subject<ProfileModel>();
    public static ReplaySubject<Unit> GConfigReload = new ReplaySubject<Unit>();
    public static Subject<Unit> Disconnect = new Subject<Unit>();
    public static Subject<object[]> ServerData = new Subject<object[]>();


    public static BoolReactiveProperty LoginSuccess = new BoolReactiveProperty(false);


    public static Subject<OnUserLogin> OnUserLogin = new Subject<OnUserLogin>();

    public static IObservable<Unit> LoginSubject
    {
        get
        {
            if (GSocket.IsLogined)
                return Observable.Return(Unit.Default);
            return GLoginObservable.AsUnitObservable();
        }
    }

    public static IObservable<Unit> LoginAndDataSuccessSubject
    {
        get
        {
            if (LoginSuccess.Value)
                return Observable.Return(Unit.Default);
            return LoginSuccess.Where(a => a).Select(a => Unit.Default).Take(1);
        }
    }
}