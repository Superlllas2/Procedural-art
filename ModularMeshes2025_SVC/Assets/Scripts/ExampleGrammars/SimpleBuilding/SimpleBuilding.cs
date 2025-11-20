using System.Collections.Generic;
using UnityEngine;

namespace Demo {
        public class SimpleBuilding : Shape {
                public int buildingHeight=-1; // The total building height (=#stocks) - value should be the same for all stocks
                public float stockHeight=1; // The height of one stock. Change this value depending on the height of your stock prefabs
                // If buildingHeight is negative, a random building height will be chosen between these two limits:
                public int maxHeight=5;
                public int minHeight=1;

                public GameObject[] stockPrefabs; // Your stock prefabs (make sure they all have the same height)
                public GameObject[] roofPrefabs; // Your roof prefabs (may have different height)

                [SerializeField]
                bool combineMeshes = true;

                bool meshesCombined = false;

                int stockNumber = 0; // The number of stocks that have already been spawned below this

		public void Initialize(int pBuildingHeight, float pStockHeight, int pStockNumber, GameObject[] pStockPrefabs, GameObject[] pRoofPrefabs) {
			buildingHeight = pBuildingHeight;
			stockHeight = pStockHeight;
			stockNumber = pStockNumber;
			stockPrefabs = pStockPrefabs;
			roofPrefabs = pRoofPrefabs;
		}

		// Returns a random game object chosen from a given gameobject array
		GameObject ChooseRandom(GameObject[] choices) {
			int index = Random.Range(0, choices.Length);
			return choices[index];
		}

                protected override void Execute() {
                        if (stockNumber==0 && Root==gameObject) {
                                ResetCombinedMeshState();
                        }
                        if (buildingHeight<0) { // This is only done once for the root symbol
                                buildingHeight = Random.Range(minHeight, maxHeight+1);
                        }

			if (stockNumber<buildingHeight) { 
				// First spawn a new stock...
				GameObject newStock = SpawnPrefab(ChooseRandom(stockPrefabs));

				//  ...and then continue with the remainder of the building, right above the spawned stock:

				// Create a new symbol - make sure to increase the y-coordinate:
				SimpleBuilding remainingBuilding = CreateSymbol<SimpleBuilding>("stock", new Vector3(0, stockHeight, 0));
				// Pass the parameters to the new symbol (component), but increase the stockNumber:
                                remainingBuilding.Initialize(buildingHeight, stockHeight, stockNumber+1, stockPrefabs, roofPrefabs);
                                remainingBuilding.combineMeshes = combineMeshes;
				// ...and continue with the shape grammar:
				remainingBuilding.Generate(buildDelay);
                        } else {
                                // Spawn a roof and stop:
                                GameObject newRoof = SpawnPrefab(ChooseRandom(roofPrefabs));

                                if (combineMeshes) {
                                        SimpleBuilding rootBuilding = Root.GetComponent<SimpleBuilding>();
                                        if (rootBuilding!=null) {
                                                rootBuilding.CombineGeneratedMeshes();
                                        }
                                }
                        }
                }

                void ResetCombinedMeshState() {
                        if (Root!=gameObject)
                                return;

                        MeshFilter rootFilter = GetComponent<MeshFilter>();
                        if (rootFilter!=null && rootFilter.sharedMesh!=null) {
                                if (Application.isPlaying) {
                                        Destroy(rootFilter.sharedMesh);
                                } else {
                                        DestroyImmediate(rootFilter.sharedMesh);
                                }
                                rootFilter.sharedMesh=null;
                        }

                        MeshRenderer rootRenderer = GetComponent<MeshRenderer>();
                        if (rootRenderer!=null) {
                                rootRenderer.sharedMaterial=null;
                        }

                        MeshCollider rootCollider = GetComponent<MeshCollider>();
                        if (rootCollider!=null) {
                                rootCollider.sharedMesh=null;
                        }

                        meshesCombined=false;
                }

                void CombineGeneratedMeshes() {
                        if (!combineMeshes || meshesCombined || Root!=gameObject)
                                return;

                        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
                        if (meshFilters==null || meshFilters.Length==0)
                                return;

                        List<CombineInstance> combineInstances = new List<CombineInstance>();
                        Material sharedMaterial = null;
                        Transform rootTransform = transform;

                        foreach (MeshFilter filter in meshFilters) {
                                if (filter.transform==rootTransform)
                                        continue;
                                if (filter.sharedMesh==null)
                                        continue;

                                MeshRenderer childRenderer = filter.GetComponent<MeshRenderer>();
                                if (childRenderer==null)
                                        continue;

                                CombineInstance instance = new CombineInstance {
                                        mesh = filter.sharedMesh,
                                        transform = rootTransform.worldToLocalMatrix * filter.transform.localToWorldMatrix
                                };
                                combineInstances.Add(instance);

                                if (sharedMaterial==null && childRenderer.sharedMaterial!=null) {
                                        sharedMaterial = childRenderer.sharedMaterial;
                                }

                                childRenderer.enabled=false;
                        }

                        if (combineInstances.Count==0)
                                return;

                        MeshFilter rootFilter = GetComponent<MeshFilter>();
                        if (rootFilter==null)
                                rootFilter = gameObject.AddComponent<MeshFilter>();

                        MeshRenderer rootRenderer = GetComponent<MeshRenderer>();
                        if (rootRenderer==null)
                                rootRenderer = gameObject.AddComponent<MeshRenderer>();

                        Mesh combinedMesh = new Mesh();
                        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
                        rootFilter.sharedMesh = combinedMesh;

                        if (sharedMaterial!=null) {
                                rootRenderer.sharedMaterial = sharedMaterial;
                        }

                        MeshCollider rootCollider = GetComponent<MeshCollider>();
                        if (rootCollider==null)
                                rootCollider = gameObject.AddComponent<MeshCollider>();
                        rootCollider.sharedMesh = null;
                        rootCollider.sharedMesh = combinedMesh;

                        meshesCombined=true;
                }
        }
}
