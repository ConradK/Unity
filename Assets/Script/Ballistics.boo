import UnityEngine

class Ballistics (MonoBehaviour):
	static _gravity = Physics.gravity.y

	def Start ():
		pass
	
	def Update ():
		pass
		
	public static def GetVerticalAim(shooter as Vector3, target as Vector3, force as single, indirect as bool) as single:
		elevation = shooter.y - target.y
		target.y = shooter.y = 0
		distance = (target - shooter).magnitude
		
		force2 = force * force
		distance2 = distance * distance
		force4 = force * force * force * force
		
		sqrt = force4 - (_gravity * (_gravity * distance2 + 2 * elevation * force2))
		
		// Not enough range
		if sqrt < 0:
			return 0.0f
		
		sqrt = Mathf.Sqrt(sqrt)
		
		lowTrajectory = Mathf.Rad2Deg * Mathf.Atan((force2 - sqrt) / (_gravity*distance));
		highTrajectory = Mathf.Rad2Deg * Mathf.Atan((force2 + sqrt) / (_gravity*distance));
		
		// Indirect uses high trajectory.
		return highTrajectory if indirect
		return lowTrajectory if not indirect
