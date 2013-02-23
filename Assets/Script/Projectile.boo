import UnityEngine

class Projectile (MonoBehaviour): 
	public TimeOut as single
	public Explosion as GameObject
	public DestroyOnImpact = true
	public ArmDelay as single
	
	def OnCollisionEnter(collision as Collision):
		contact = collision.contacts[0]
		if Explosion:
			rotation = Quaternion.FromToRotation(Vector3.up, contact.normal)
			Instantiate(Explosion, contact.point, rotation)
			
		if DestroyOnImpact:
			DestroyNow()
			
	def Awake():
		Invoke("DestroyNow", TimeOut)
		Invoke("Arm", ArmDelay)
		
	def DestroyNow():
		Destroy(gameObject)
		
	def Arm():
		rigidbody.collider.isTrigger = false
			
	def Start ():
		pass
	
	def Update ():
		pass
