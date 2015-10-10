using AlumnoEjemplos.Manaos_Games;
//using Microsoft.DirectX.Direct3D;
//using Microsoft.DirectX.DirectInput;
using TgcViewer;
using TgcViewer.Example;
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Terrain;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;

namespace AlumnoEjemplos.MiGrupo
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploAlumno : TgcExample
    {
        /// <summary>
        /// Categor�a a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el �rbol de la derecha de la pantalla.
        /// </summary>
        /// 
        ManejadorColisiones colisionador;
        SphereCollisionManager collisionManager;

        TgcPickingRay pickingRay;
        Vector3 selectionPoint;
        bool applyMovement;
        bool selected;
        TgcMesh selectedMesh;
        Nivel1 nivel;
        TgcArrow directionArrow;

        Camara fpsCamara;

        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        /// <summary>
        /// Completar nombre del grupo en formato Grupo NN
        /// </summary>
        public override string getName()
        {
            return "Manaos Games";
        }

        /// <summary>
        /// Completar con la descripci�n del TP
        /// </summary>
        public override string getDescription()
        {
            return " - Se trata de resolver puzzles jugando con las perspectivas de los objetos.";
        }

        /// <summary>
        /// M�todo que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aqu� todo el c�digo de inicializaci�n: cargar modelos, texturas, modifiers, uservars, etc.
        /// Borrar todo lo que no haga falta
        /// </summary>
        public override void init()
        {
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            Microsoft.DirectX.Direct3D.Device d3dDevice = GuiController.Instance.D3dDevice;
            TgcSceneLoader loader = new TgcSceneLoader();

            nivel = new Nivel1();
  
            fpsCamara = new Camara();

            GuiController.Instance.CurrentCamera = fpsCamara;

            fpsCamara.MovementSpeed = 1000f;
            fpsCamara.RotationSpeed = 2f;
            
            fpsCamara.setCamera(nivel.posicionInicial(), nivel.orientacionCamara());
            
            fpsCamara.updateCamera();


            colisionador = new ManejadorColisiones(fpsCamara, nivel.Obstaculos);
            
            //Crear manejador de colisiones
            collisionManager = new SphereCollisionManager();
            collisionManager.GravityEnabled = true;

 
            //Iniciarlizar PickingRay
            pickingRay = new TgcPickingRay();

            //Flecha para marcar la direcci�n
            directionArrow = new TgcArrow();
            directionArrow.Thickness = 5;
            directionArrow.HeadSize = new Vector2(10, 10);
        }


        /// <summary>
        /// M�todo que se llama cada vez que hay que refrescar la pantalla.
        /// Escribir aqu� todo el c�digo referido al renderizado.
        /// Borrar todo lo que no haga falta
        /// </summary>
        /// <param name="elapsedTime">Tiempo en segundos transcurridos desde el �ltimo frame</param>
        public override void render(float elapsedTime)
        {
            //colisionador.update();

            collisionManager.moveCamera(fpsCamara, nivel.Obstaculos);           

            //Si hacen clic con el mouse, ver si hay colision RayAABB
            if (GuiController.Instance.D3dInput.buttonPressed(TgcViewer.Utils.Input.TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Actualizar Ray de colisi�n en base a posici�n del mouse
                pickingRay.updateRay();

                                //Testear Ray contra el AABB de todos los meshes
                foreach (TgcMesh objeto in nivel.Seleccionables)
                {
                    TgcBoundingBox aabb = objeto.BoundingBox;

                    //Ejecutar test, si devuelve true se carga el punto de colision collisionPoint
                    selected = TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, aabb, out selectionPoint);

                    if (selected)
                    {
                        selectedMesh = objeto;
                        //Fijar nueva posici�n destino
                        applyMovement = true;

                        directionArrow.PEnd = new Vector3(selectionPoint.X, selectionPoint.Y, selectionPoint.Z);
                        
                        break;
                    }
                }
            }

            //Interporlar movimiento, si hay que mover
            if (applyMovement)
            {
                //Ver si queda algo de distancia para mover
                Vector3 posDiff = selectionPoint - selectedMesh.BoundingBox.calculateBoxCenter();
                float posDiffLength = posDiff.LengthSq();
                if (posDiffLength > float.Epsilon)
                {
                    //Movemos el mesh interpolando por la velocidad
                    float currentVelocity = 100 * elapsedTime;
                    posDiff.Normalize();
                    posDiff.Multiply(currentVelocity);

                    //Ajustar cuando llegamos al final del recorrido
                    Vector3 newPos = selectedMesh.BoundingBox.calculateBoxCenter() + posDiff;
                    if (posDiff.LengthSq() > posDiffLength)
                    {
                        newPos = selectionPoint;
                    }

                    //Actualizar flecha de movimiento
                    directionArrow.PStart = new Vector3(selectedMesh.Position.X, selectedMesh.Position.Y, selectedMesh.Position.Z);
                    directionArrow.updateValues();

                    //Actualizar posicion del mesh
                    selectedMesh.Position = newPos;

                    //Como desactivamos la transformacion automatica, tenemos que armar nosotros la matriz de transformacion
                    selectedMesh.Transform = Matrix.Translation(selectedMesh.Position);
                }
                //Se acabo el movimiento
                else
                {
                    applyMovement = false;
                }
            }

            nivel.Render();

            if (selected)
                selectedMesh.BoundingBox.render();
        }

        /// <summary>
        /// M�todo que se llama cuando termina la ejecuci�n del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {

        }

    }
}
