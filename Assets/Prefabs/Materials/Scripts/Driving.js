var leftWheel: WheelCollider;
var rightWheel: WheelCollider;
var maxBrakeTorque= 500.0;
var maxTorque = 150.0;
var GuiSpeed: GUIText;
var flWheel : Transform;
var frWheel : Transform;
var rlWheel : Transform;
var rrWheel : Transform;
var currentSpeed : float =0.0;
var maxSpeed : float = 200.0;
var maxBackwardSpeed : float = 40.0;
private var isBraking : boolean=false;


private var currentGear: int=0;
var guiSpeedPointer : Texture2D;
var guiSpeedDisplay : Texture2D;
var gearSpeed : int [];
function Start() {
GuiSpeed.material.color = Color.green;
rigidbody.centerOfMass.y = 0;
}

function  FixedUpdate () {
currentSpeed = (Mathf.PI * 2 * leftWheel.radius) * leftWheel.rpm * 60 /1000;
currentSpeed = Mathf.Round(currentSpeed);

if (((currentSpeed> 0) && (Input.GetAxis("Vertical") <0)) || ((currentSpeed< 0) && (Input.GetAxis("Vertical") >0))) {
isBraking = true;
}

else  {
isBraking = false;
leftWheel.brakeTorque =0;
rightWheel.brakeTorque =0;
}

if (isBraking == false) {
if ((currentSpeed < maxSpeed) && (currentSpeed >(maxBackwardSpeed *-1))) {
leftWheel.motorTorque = maxTorque * Input.GetAxis("Vertical");
rightWheel.motorTorque = maxTorque * Input.GetAxis("Vertical");
}
else {
leftWheel.motorTorque = 0;
rightWheel.motorTorque = 0;
}
} 

else {
leftWheel.brakeTorque = maxBrakeTorque;
rightWheel.brakeTorque = maxBrakeTorque;
leftWheel.motorTorque =  0;
rightWheel.motorTorque =  0;		
}

leftWheel.steerAngle = 10 * Input.GetAxis("Horizontal");
rightWheel.steerAngle = 10 * Input.GetAxis("Horizontal");
GuiSpeed.text = currentSpeed.ToString();


}

function Fullbraking (){


}

function Update () {
RotateWheels();
SteelWheels();
GearSound();
}

function OnGUI () {
	var pointerPosition: float = 40.0;
	
   GUI.Box(Rect(0.0,0.0,140.0,140.0),guiSpeedDisplay);
   if (currentSpeed>0) {
   pointerPosition = currentSpeed + 40;
   }
   GUIUtility.RotateAroundPivot(pointerPosition,Vector2(70,70));
   GUI.DrawTexture(Rect(0.0,0.0,140.0,140.0),guiSpeedPointer, ScaleMode.StretchToFill,true,0);
   }
   
   function RotateWheels() {
flWheel.Rotate(leftWheel.rpm / 60 * 360 * Time.deltaTime ,0,0);
frWheel.Rotate(leftWheel.rpm / 60 * 360 * Time.deltaTime ,0,0);
rlWheel.Rotate(leftWheel.rpm / 60 * 360 * Time.deltaTime ,0,0);
rrWheel.Rotate(leftWheel.rpm / 60 * 360 * Time.deltaTime ,0,0);
}

function SteelWheels() {
flWheel.localEulerAngles.y = leftWheel.steerAngle - flWheel.localEulerAngles.z ;
frWheel.localEulerAngles.y = rightWheel.steerAngle - frWheel.localEulerAngles.z ;
}

function SetCurrentGear() {
var gearNumber : int;
gearNumber = gearSpeed.Length;

for (var i=0; i < gearNumber;i++){
if (gearSpeed[i] > currentSpeed) {
currentGear = i;
break;

}
}
}
function GearSound() {
var tempMinSpeed:float=0.00;
var tempMaxSpeed:float=0.00;
var currentPitch:float=0.00;
switch (currentGear){
case 0:
	tempMinSpeed=0.00;
	tempMaxSpeed=gearSpeed[currentGear];
	break;
	
	default:
	tempMinSpeed=gearSpeed[currentGear -1];
	tempMaxSpeed=gearSpeed[currentGear];

}
currentPitch=((Mathf.Abs(currentSpeed) - tempMinSpeed)/(tempMaxSpeed-tempMinSpeed)) -5;
audio.pitch = currentPitch ;
}