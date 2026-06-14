using UnityEngine;
using CasualtiesUnknown.Stats.Core;

namespace CasualtiesUnknown.Stats.Triggers
{
    /// <summary>每帧累计本地玩家水平位移到统计；以厘米为单位入盘避免浮点累计误差。</summary>
    internal static class MovementTracker
    {
        private const float ArmGraceSeconds = 1.5f;
        private const float MaxFrameDeltaMeters = 8f;
        private const float MinFrameDeltaMeters = 0.005f;

        private static float _armedAt;
        private static bool _armed;
        private static bool _hasLast;
        private static float _lastX;
        private static float _residualMeters;

        internal static void Arm()
        {
            _armed = true;
            _hasLast = false;
            _armedAt = Time.unscaledTime;
        }

        internal static void Tick()
        {
            if (!_armed) return;
            if (Time.unscaledTime - _armedAt < ArmGraceSeconds) return;
            var cam = PlayerCamera.main;
            if (cam == null || cam.body == null) { _hasLast = false; return; }
            float x = cam.body.transform.position.x;
            if (!_hasLast) { _lastX = x; _hasLast = true; return; }
            float d = Mathf.Abs(x - _lastX);
            _lastX = x;
            if (d < MinFrameDeltaMeters || d > MaxFrameDeltaMeters) return;
            _residualMeters += d;
            int wholeCm = (int)(_residualMeters * 100f);
            if (wholeCm <= 0) return;
            _residualMeters -= wholeCm / 100f;
            StatsManager.AddMovedCm(wholeCm);
        }
    }
}
