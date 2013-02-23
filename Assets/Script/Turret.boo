import UnityEngine

class Turret (MonoBehaviour): 
	public TargetObject as Transform
	public RotationSpeed as single
	public ElevationSpeed as single // 0 means fixed barrel angle, such as with a missile launcher.
	public Barrels as Transform
	
	public ProjectileForce as single
	public Projectile as GameObject
	public RateOfFire as single
	public NumberOfProjectiles as int
	public Deviation as single
	public Indirect as bool
	
	_barrels as (Barrel)
	_origin as Vector3
	_init = false

	def OnDrawGizmosSelected():
		Gizmos.color = Color.cyan
		if not _init:
			return
			
		for barrel in _barrels:
			Gizmos.DrawSphere(barrel.GetProjectileOrigin(), 0.1)
			
		targetPosition = GetTargetPoint()
		Gizmos.DrawSphere(targetPosition, 0.5)
		Gizmos.DrawSphere(_origin, 0.1)
	

	def Start():
		_barrels = GetComponents of Barrel()
		
		for barrel in _barrels:
			barrel.Init(self)
			
		_init = true
		
	def FixedUpdate():
		if not TargetObject or not _init:
			return
		
		
		if (targetPoint = GetTargetPoint()) == null:
			return 

		// Vector that looks at targetPosition. Converted into local space to account for parent transform rotations.
		firingVector = transform.parent.InverseTransformPoint(targetPoint) - transform.parent.InverseTransformPoint(_origin)
		fire = true

		// Rotate on the Y axis to align with firingVector
		yRotation = Quaternion.LookRotation(firingVector, transform.parent.up)
		yRotation = Quaternion.Euler(0, yRotation.eulerAngles.y, 0)
		transform.localRotation = Quaternion.RotateTowards(transform.localRotation, yRotation, RotationSpeed)
		
		if Quaternion.Angle(transform.localRotation, yRotation) > Deviation:
			fire = false
		
		if not ElevationSpeed == 0:
			// Rotate the barrels on their X axis to align with firingVector
			xRotation = Quaternion.LookRotation(firingVector, Vector3.right)
			xRotation = Quaternion.Euler(xRotation.eulerAngles.x, 0, 0)
			Barrels.localRotation = Quaternion.RotateTowards(Barrels.localRotation, xRotation, ElevationSpeed)
			
			if Quaternion.Angle(Barrels.localRotation, xRotation) > Deviation:
				fire = false
			
		if fire:
			for barrel in _barrels:
				barrel.BeginFiring()
		else:
			for barrel in _barrels:
				barrel.CeaseFiring()

								
	def GetTargetPoint():
		targetPosition = TargetObject.position
		
		// Projectile origin. Is between the (assumed) two barrels.
		originDiff = _barrels[0].GetProjectileOrigin() - _barrels[1].GetProjectileOrigin()
		_origin = _barrels[0].GetProjectileOrigin() - originDiff * 0.5
		
		// Angle needed to hit the target with the given force.
		targetAngle = Ballistics.GetVerticalAim(_origin, targetPosition, ProjectileForce, Indirect)
		
		return null if targetAngle == 0
		
//		solutionTime = CalculateFlightTime(solutionAngle * Mathf.Rad2Deg);

		// Vector that points from _origin to targetPosition		
		targetVector = targetPosition - _origin
		targetVector.y = 0
		
		// Find the "right" of the targetVector to rotate around
		targetVectorRight = Vector3.Cross(targetVector, Vector3.down)
		// Rotate the targetVector around its own "right" to get the target point
		targetVector = Quaternion.AngleAxis(targetAngle, targetVectorRight) * targetVector
		
		// The final target point. A shot fired at this point in world space will hit targetPosition
		return _origin + targetVector

		