using Sackrany.Actor.DefaultFeatures.BehaviourFeature.Modules;
using Sackrany.Actor.Traits.Damage;
using Sackrany.Actor.UnitMono;

using UnityEngine;

namespace Sackrany.Actor.DefaultFeatures.Visual
{
    [RequireComponent(typeof(Renderer))]
    public class UnitHealthBarShader : MonoBehaviour
    {
        public float FlowDelay = 0.5f;
        public float FadeDelay = 3f;
        
        public float HitSpeed = 25f;
        public float ScaleSpeed = 25f;

        int OPACITY_ID;
        int VALUE_ID;
        int VALUE_FLOW_ID;
        int VALUE_HIT_ID;
        int VALUE_SCALE_ID;

        BigHealthBehaviourModule _bigHealthBehaviourModule;
        Unit unit;
        Renderer renderer;
        
        Vector3 _offset;
        Vector3 _scale;
        
        bool initialized;
        void Awake()
        {
            _scale = transform.localScale;
            _offset = transform.localPosition;
            
            OPACITY_ID = Shader.PropertyToID("_Opacity");
            VALUE_ID = Shader.PropertyToID("_Value");
            VALUE_FLOW_ID = Shader.PropertyToID("_ValueFlow");
            VALUE_HIT_ID = Shader.PropertyToID("_ValueHit");
            VALUE_SCALE_ID = Shader.PropertyToID("_ValueScale");
            
            unit = GetComponentInParent<Unit>();
            renderer = GetComponent<Renderer>();
            
            unit.OnStartWorking += Init;
            if (unit.IsActive) Init(unit);
            
            unit.OnRestart += OnUnitReset;
        }
        void OnUnitReset(Unit obj)
        {
            transform.SetParent(obj.IsActive ? null : unit.transform, true);
            transform.localScale = _scale;
        }
        void Init(Unit obj)
        {
            initialized = unit.TryGet(out _bigHealthBehaviourModule);
            if (!initialized) return;

            _bigHealthBehaviourModule.OnDamaged += OnDamaged;
            health = 1;
            flow = 1;
            opacity = 0;
            
            transform.SetParent(null, true);
            transform.localScale = _scale;
        }
        void OnDamaged(BigDamageInfo value)
        {
            health = _bigHealthBehaviourModule.Health / _bigHealthBehaviourModule.MaxHealth;
            opacity = 1;
            opacityTimer = FadeDelay;
            flowTimer = FlowDelay;
            hitTimer = 1;
            scaleTimer = 1;
        }
        
        float opacity = 1;
        float opacityTimer;
        float health;
        float flow;
        float flowTimer;
        float hitTimer;
        float scaleTimer;
        void Update()
        {
            if (!initialized) return;
            transform.position = unit.transform.position + _offset;

            if (opacityTimer <= 0)
                opacity = Mathf.Lerp(opacity, 0, Time.deltaTime * 5f);
            else opacityTimer -= Time.deltaTime;
            
            renderer.materials[0].SetFloat(OPACITY_ID, opacity);
            renderer.materials[0].SetFloat(VALUE_ID, health);
            renderer.materials[0].SetFloat(VALUE_FLOW_ID, flow);
            renderer.materials[0].SetFloat(VALUE_HIT_ID, hitTimer);
            renderer.materials[0].SetFloat(VALUE_SCALE_ID, scaleTimer);
            
            if (flowTimer <= 0)
                flow = Mathf.Lerp(flow, health, Time.deltaTime * 15);
            else flowTimer -= Time.deltaTime;
            
            hitTimer = Mathf.Lerp(hitTimer, 0, Time.deltaTime * HitSpeed);
            scaleTimer = Mathf.Lerp(scaleTimer, 0, Time.deltaTime * ScaleSpeed);
        }
    }
}