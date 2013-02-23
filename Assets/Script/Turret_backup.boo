import UnityEngine

class Turret_backup (MonoBehaviour): 
	public TargetObject as Transform
	public RotationSpeed as single
	public ElevationSpeed as single // 0 means fixed barrel angle, such as with a missile launcher.
	public Barrels as Transform
	
	public ProjectileForce as single
	public Projectile as GameObject
	public RateOfFire as single
	public NumberOfProjectiles as int
	public Deviation as single
	
	_barrels as (Barrel)
	_origin as Vector3
	_init = false

	def OnDrawGizmosSelected():
		Gizmos.color = Color.cyan
		if not _barrels:
			return
			
		for barrel in _barrels:
			Gizmos.DrawSphere(barrel.GetProjectileOrigin(), 0.1)
			
		Gizmos.DrawSphere(_origin, 0.1)
	

	def Start():
		_barrels = GetComponents[of Barrel]()
		
		//for barrel in _barrels:
//			barrel.Init(self)
			
		_init = true
		
	def FixedUpdate():
		if not TargetObject or not _init:
			return

		targetPosition = TargetObject.position
		
		originDiff = _barrels[0].GetProjectileOrigin() - _barrels[1].GetProjectileOrigin()
		_origin = _barrels[0].GetProjectileOrigin() - originDiff * 0.5
		
		fire = true
						
		// Rotate on the Y axis to face target
		facingVector = targetPosition - _origin;
		yRotation = Quaternion.Euler(0, Quaternion.LookRotation(facingVector, transform.parent.up).eulerAngles.y, 0)
		transform.localRotation = Quaternion.RotateTowards(transform.localRotation, yRotation, RotationSpeed)
		
		if Quaternion.Angle(transform.localRotation, yRotation) > Deviation:
			fire = false
		
		if not ElevationSpeed == 0:
			// Rotate the barrels on their X axis to firing angle
			xAngle = Ballistics.GetVerticalAim(_origin, targetPosition, ProjectileForce, false)
			
			xRotation = Quaternion.AngleAxis(xAngle, Vector3.right)
			Barrels.localRotation = Quaternion.RotateTowards(Barrels.localRotation, xRotation, ElevationSpeed)
			
			if Quaternion.Angle(Barrels.localRotation, xRotation) > Deviation:
				fire = false
			
		if fire:
			for barrel in _barrels:
				barrel.BeginFiring()
		else:
			for barrel in _barrels:
				barrel.CeaseFiring()

		