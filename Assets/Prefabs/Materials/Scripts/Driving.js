var leftWheel: WheelCollider;
var rightWheel: WheelCollider;
var maxTorque = 238.0;
var GuiSpeed: GUIText;

private var currentSpeed = 0.0;

var guiSpeedPointer : Texture2D;
var guiSpeedDisplay : Texture2D;
function Start() {
GuiSpeed.material.color = Color.green;
rigidbody.centerOfMass.y = 0;
}

function  FixedUpdate () {
currentSpeed = (Mathf.PI * 2 * leftWheel.radius) * leftWheel.rpm * 60 /1000;
currentSpeed = Mathf.Round(currentSpeed);
leftWheel.motorTorque = maxTorque * Input.GetAxis("Vertical");
rightWheel.motorTorque = maxTorque * Input.GetAxis("Vertical");

leftWheel.steerAngle = 10 * Input.GetAxis("Horizontal");
rightWheel.steerAngle = 10 * Input.GetAxis("Horizontal");
GuiSpeed.text = currentSpeed.ToString();

}

function OnGUI () {
   GUI.Box(Rect(0.0,0.0,140.0,140.0),guiSpeedDisplay);
   GUIUtility.RotateAroundPivot(currentSpeed + 40,Vector2(70,70));
   GUI.DrawTexture(Rect(0.0,0.0,140.0,140.0),guiSpeedPointer, ScaleMode.StretchToFill,true,0);
   }