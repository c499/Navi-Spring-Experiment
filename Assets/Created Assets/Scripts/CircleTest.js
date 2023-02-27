// ring mesh emitter by Jason Kowalin (Jay Kay, alucardj) Jan 7th 2013
 #pragma strict
 
public var radiusBegin : float = 0.1;
public var radiusEnd : float = 5.0;
public var expandTime : float = 5.0;
 
public var ringWidth : float = 0.2;
public var segments : int = 60;
 
private var theEmitter : ParticleEmitter;
private var isExpanding : boolean = false;
private var eTimer : float = 0.0;
private var eStep : float = 0.0;
 
function Start() 
{
    theEmitter = GetComponent( ParticleEmitter );
    theEmitter.emit = false; // start with no particles emitted
     
    StartMesh(); // build the ring
}
 
function Update() 
{
    if ( Input.GetMouseButtonDown(0) )
    {
        theEmitter.emit = true; // start emitting particles
        isExpanding = true; // start expanding
        eTimer = 0.0; // set timer to zero
        eStep = ( radiusEnd - radiusBegin ) / expandTime; // calculate step for expanding ring
        radiusInner = radiusBegin; // set the start radius
    }
     
    if ( isExpanding )
    {
        ExpandRing();
    }
}
 
function ExpandRing() 
{
    eTimer += Time.deltaTime; // update timer
     
    if ( eTimer > expandTime ) // check if ring has finished expanding
    {
        theEmitter.emit = false; // stop emmiting particles
        isExpanding = false; // stop expanding
    }
     
     
    // -- shouldn't have to change any of this code --
     
    // update vertices
    radiusInner += eStep * Time.deltaTime;
    radiusOuter = radiusInner + ringWidth;
     
    for (var i : int = 0; i < (segments + 1); i ++)
    {
        //vertsInner[ i ] = new Vector3( posX[i], posY[i], 0.0 ) * radiusInner; // vertical ring
        //vertsOuter[ i ] = new Vector3( posX[i], posY[i], 0.0 ) * radiusOuter;
         
        vertsInner[ i ] = new Vector3( posX[i], 0.0, posY[i] ) * radiusInner; // horizontal ring
        vertsOuter[ i ] = new Vector3( posX[i], 0.0, posY[i] ) * radiusOuter;
    }
     
vertsInner[ segments ] = vertsInner[0];
vertsOuter[ segments ] = vertsOuter[0];
     
verts = new Vector3[ ( segments + 1 ) * 2 ];
     
for (i = 0; i < segments + 1; i ++ )
{
    verts[i] = vertsOuter[i];
    verts[ i + segments + 1 ] = vertsInner[i];
}
     
mesh.vertices = verts;
     
// --
     
}
 
 
// ---- Don't change anything past this line !! ----
 
 
private var calcAngle : float;
private var posX : float[];
private var posY : float[];
 
private var radiusInner : float = 0.1;
private var radiusOuter : float = 2.0;
 
private var vertsInner : Vector3[];
private var vertsOuter : Vector3[];
 
private var mesh : Mesh;
 
private var uv : Vector2[];
private var verts : Vector3[];
private var tris : int[];
 
function StartMesh() 
{
    if ( !mesh )
    {
        mesh = new Mesh();
        GetComponent(MeshFilter).mesh = mesh;
        mesh.name = "ParticleMesh";
    }    
    Construct();
}
 
function Construct() 
{
    var i : int = 0;
    // ----
     
    calcAngle = 0;
    posX = new float[segments + 1];
    posY = new float[segments + 1];
     
    // Calculate Circle on X-Y
    for (i = 0; i < segments + 1; i ++)
    {
        posX[i] = Mathf.Sin( calcAngle * Mathf.Deg2Rad );
        posY[i] = Mathf.Cos( calcAngle * Mathf.Deg2Rad );
         
        calcAngle += 360.0 / parseFloat(segments);
    }
     
    // ----
     
    // Vertices
    vertsInner = new Vector3[ ( segments + 1 ) ];
    vertsOuter = new Vector3[ ( segments + 1 ) ];
     
    var radiusInner : float = radiusBegin;
    radiusOuter = radiusBegin + ringWidth;
     
    for (i = 0; i < (segments + 1); i ++)
    {
        //vertsInner[ i ] = new Vector3( posX[i], posY[i], 0.0 ) * radiusInner; // vertical ring
        //vertsOuter[ i ] = new Vector3( posX[i], posY[i], 0.0 ) * radiusOuter;
         
        vertsInner[ i ] = new Vector3( posX[i], 0.0, posY[i] ) * radiusInner; // horizontal ring
        vertsOuter[ i ] = new Vector3( posX[i], 0.0, posY[i] ) * radiusOuter;
    }
     
    vertsInner[ segments ] = vertsInner[0];
    vertsOuter[ segments ] = vertsOuter[0];
     
    verts = new Vector3[ ( segments + 1 ) * 2 ];
     
    for (i = 0; i < segments + 1; i ++ )
    {
        verts[i] = vertsOuter[i];
        verts[ i + segments + 1 ] = vertsInner[i];
    }
     
    // ----
     
    // UVs
    uv = new Vector2[verts.length];
     
    for (i = 0; i < segments + 1; i ++ )
    {
        uv[i] = Vector2( 1.0 - (parseFloat(i) * (1.0 / segments)), 1.0 );
        uv[ i + segments + 1 ] = Vector2( 1.0 - (parseFloat(i) * (1.0 / segments)), 0.0 );
    }
     
    // ----
     
    // Triangles
    tris = new int[ segments * 2 * 3 ];
     
    var index : int = 0;
    for (i = 0; i < tris.length / 6; i ++ )
    {
        tris[index + 0] = i;
        tris[index + 2] = i + 1;
        tris[index + 1] = i + segments + 1;
         
        tris[index + 3] = i + 1;
        tris[index + 5] = i + segments + 1 + 1;
        tris[index + 4] = i + segments + 1;
         
        index += 6;
    }
     
    // ----
     
    // assign mesh
    mesh.vertices = verts; 
    mesh.uv = uv;
    mesh.triangles = tris;
     
    mesh.RecalculateBounds();
    mesh.RecalculateNormals();
}
 
 
// ----
 
// http://answers.unity3d.com/questions/375604/expanding-ring-velocity-ellipsoid-particle-emitter.html