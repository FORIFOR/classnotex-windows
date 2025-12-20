using System;
using System.Collections.Generic;
using ClassnoteX.Core.Interfaces;
using ClassnoteX.Infrastructure.Services;
using ClassnoteX.Infrastructure.Repositories;

namespace ClassnoteX.WinUI.Services;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new();

    public static void Initialize()
    {
        // Register Services
        _services[typeof(IAudioRecorder)] = new AudioRecorderService();
        _services[typeof(INavigationService)] = new NavigationService();
        _services[typeof(ISessionRepository)] = new ClassnoteX.Infrastructure.Repositories.SessionRepository();
        
        // Sync (Manually injecting for now, DI container would be better)
        var repo = (ISessionRepository)_services[typeof(ISessionRepository)];
        _services[typeof(ISyncService)] = new ClassnoteX.Infrastructure.Services.SyncService(repo);

        // STT (Using Mock for now since we don't have native dlls)
        _services[typeof(ISttService)] = new ClassnoteX.Infrastructure.Services.MockSttService();
        _services[typeof(ClassnoteX.Infrastructure.Services.ModelManagerService)] = new ClassnoteX.Infrastructure.Services.ModelManagerService();
    }

    public static T GetService<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }
        throw new InvalidOperationException($"Service {typeof(T).Name} not registered.");
    }
}
