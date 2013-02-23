import UnityEngine

class Barrel (MonoBehaviour):
	public Delay as single
	public BarrelObject as Transform
	
	_turret as Turret
	_firing = false
	
	//_count2 = 0
	
	def Start ():
		pass
		
	def Update ():
		pass
	
	def Init(turret as Turret):
		_turret = turret
	
	def BeginFiring():
		if not _firing:
			_firing = true
			//Debug.Log("Begin " + Time.time + " Delay " + Delay + " Rate of Fire " + _turret.RateOfFire)
			InvokeRepeating("Fire", Delay, _turret.RateOfFire)
		
	def CeaseFiring():
		if _firing:
			_firing = false
			CancelInvoke()
		
	def Fire():
		//Debug.Log("Fire " + Delay + " Count2 " + _count2 + " Time " + Time.time)
		//_count2 += 1
		for i in range(_turret.NumberOfProjectiles):
			projectile = Instantiate(_turret.Projectile, BarrelObject.position, BarrelObject.localRotation);
			velocity = BarrelObject.TransformDirection(Vector3(Random.Range(-_turret.Deviation, _turret.Deviation), Random.Range(-_turret.Deviation, _turret.Deviation), _turret.ProjectileForce));
			projectile.rigidbody.velocity = velocity;
			
	def GetProjectileOrigin() as Vector3:
		return BarrelObject.position
		
