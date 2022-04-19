using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Utilities;

[RequireComponent(typeof(PoolableObject))]
public abstract class AspidShotBase : EnemyProjectile
{
	[Header("Aspid Shot Info")]
	[SerializeField]
	string startAnimation = "Idle";
	[SerializeField]
	string deathAnimation = "Impact";
	[SerializeField]
	AudioClip ImpactClip;
	[SerializeField]
	float lifeTime = 5f;
	[SerializeField]
	[Tooltip("Delay before the Aspid Shot can be destroyed by hitting objects")]
	float hitDelay = 0.1f;
	[SerializeField]
	[Tooltip("The delay before the aspid shot is sent back to the pooling system")]
	float destructionDelay = 0f;


	protected new SpriteRenderer light;
	WeaverAnimationPlayer anim;
	protected PoolableObject poolComponent;
	protected SpriteRenderer mainRenderer;

	float _hitTimer = 0f;

	[WeaverCore.Attributes.ExcludeFieldFromPool]
	Color oldColor;

	public WeaverAnimationPlayer Animator
	{
		get
		{
			if (anim == null)
			{
				anim = GetComponent<WeaverAnimationPlayer>();
			}
			return anim;
		}
	}

	protected override void Awake()
	{
		//Debug.Log("Hit Timer = " + _hitTimer);
		StopAllCoroutines();
		if (light == null)
		{
			mainRenderer = GetComponent<SpriteRenderer>();
			anim = GetComponent<WeaverAnimationPlayer>();
			poolComponent = GetComponent<PoolableObject>();
			light = transform.Find("Light").GetComponent<SpriteRenderer>();
			oldColor = light.color;
		}
		mainRenderer.enabled = true;
		light.enabled = true;
		light.color = oldColor;
		poolComponent.ReturnToPool(lifeTime);
		if (!string.IsNullOrEmpty(startAnimation))
		{
			anim.PlayAnimation(startAnimation);
		}
		base.Awake();
	}

	protected override void Update()
	{
		if (_hitTimer < hitDelay)
		{
			_hitTimer += Time.deltaTime;
		}
		base.Update();
	}

	protected override void OnHit(GameObject collision)
	{
		if (_hitTimer >= hitDelay)
		{
			//Debug.Log("ASPID SHOT COLLISION");
			base.OnHit(collision);
			AspidImpact();
		}
	}

	public void AspidImpact()
	{
		if (!string.IsNullOrEmpty(deathAnimation))
		{
			anim.PlayAnimation(deathAnimation);
		}
		WeaverAudio.PlayAtPoint(ImpactClip, transform.position);
		StartCoroutine(Fader());
		OnImpact();
	}

	protected virtual void OnImpact()
	{

	}

	IEnumerator Fader()
	{
		var oldLightColor = light.color;
		var newColor = new Color(oldLightColor.r, oldLightColor.g, oldLightColor.b, 0f);


		var animationTime = anim.AnimationData.GetClipDuration(anim.PlayingClip);


		for (float i = 0; i < animationTime; i += Time.deltaTime)
		{
			light.color = Color.Lerp(oldLightColor, newColor, i / animationTime);
			yield return null;
		}

		yield return anim.WaitforClipToFinish();

		//Debug.Log("ASPID SHOT DESTROYED");

		mainRenderer.enabled = false;
		light.enabled = false;

		poolComponent.ReturnToPool(destructionDelay);
	}

	protected override void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Hero Box"))
		{
			base.OnTriggerEnter2D(collision);
		}
	}

	protected override void OnTriggerStay2D(Collider2D collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Hero Box"))
		{
			base.OnTriggerStay2D(collision);
		}
	}
}
