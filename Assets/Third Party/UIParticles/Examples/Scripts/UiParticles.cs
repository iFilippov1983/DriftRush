using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;
// ReSharper disable InconsistentNaming

// ReSharper disable once CheckNamespace
namespace UiParticles
{
    /// <summary>
    /// Ui Parcticles, requiere ParticleSystem component
    /// </summary>
    [RequireComponent (typeof(ParticleSystem))]
	public class UiParticles : MaskableGraphic
	{

		#region InspectorFields
	    
		/// <summary>
		/// ParticleSystem used for generate particles
		/// </summary>
		[SerializeField]
	    [FormerlySerializedAs ("m_ParticleSystem")]
		private ParticleSystem m_ParticleSystem;

	    /// <summary>
	    /// If true, particles renders in streched mode
	    /// </summary>
	    [FormerlySerializedAs ("m_RenderMode")]
	    [SerializeField]
	    [Tooltip("Render mode of particles")]
	    private UiParticleRenderMode m_RenderMode  = UiParticleRenderMode.Billboard;

	    /// <summary>
	    /// Scale particle size, depends on particle velocity
	    /// </summary>
	    [FormerlySerializedAs ("m_StretchedSpeedScale")]
	    [SerializeField]
	    [Tooltip("Speed Scale for streched billboards")]
	    private float m_StretchedSpeedScale = 1f;

	    /// <summary>
	    /// Sclae particle length in streched mode
	    /// </summary>
	    [FormerlySerializedAs ("m_StretchedLenghScale")]
	    [SerializeField]
	    [Tooltip("Speed Scale for streched billboards")]
	    private float m_StretchedLenghScale = 1f;


		[FormerlySerializedAs ("m_IgnoreTimescale")]
		[SerializeField]
		[Tooltip("If true, particles ignore timescale")]
		private bool m_IgnoreTimescale = false;

		[SerializeField]
		private Mesh m_RenderedMesh;

	    #endregion
		
		
		#region Public properties
		/// <summary>
		/// ParticleSystem used for generate particles
		/// </summary>
		/// <value>The particle system.</value>
		public ParticleSystem ParticleSystem {
			get { return m_ParticleSystem; }
			set {
				if (SetPropertyUtility.SetClass (ref m_ParticleSystem, value))
					SetAllDirty ();
			}
		}

		/// <summary>
		/// Texture used by the particles
		/// </summary>
		public override Texture mainTexture {
			get
			{
				if (material != null)
				{
					if (m_MainTexProperty == -1)
					{
						if (material.HasProperty(MainTexPropertyName))
							m_MainTexProperty = Shader.PropertyToID(MainTexPropertyName);
						else if (material.HasProperty(BaseMapPropertyName))
							m_MainTexProperty = Shader.PropertyToID(BaseMapPropertyName);
					}

					return material.GetTexture(m_MainTexProperty);
				}

				return s_WhiteTexture;
			}
		}

        /// <summary>
        /// Particle system render mode (billboard, strechedBillobard)
        /// </summary>
	    public UiParticleRenderMode RenderMode
	    {
	        get { return m_RenderMode; }
	        set
	        {
	            if(SetPropertyUtility.SetStruct(ref m_RenderMode, value))
	                SetAllDirty();
	        }
	    }

		public Mesh RenderedMesh
		{
			get { return m_RenderedMesh; }
			set
			{
				if (SetPropertyUtility.SetClass(ref m_RenderedMesh, value))
				{
					InitMeshData();
					SetAllDirty();
				}
			}
		}
			
		#endregion

		private const string MainTexPropertyName = "_MainTex";
		private const string BaseMapPropertyName = "_BaseMap";
		private int m_MainTexProperty = -1;
		private Geometry _geometry;
		
		private ParticleSystemRenderer m_ParticleSystemRenderer;
		private ParticleSystem.Particle[] m_Particles;

		private Mesh _cachedMesh;
		private Vector3[] m_MeshVerts;
		private int[] m_MeshTriangles;
		private Vector2[] m_MeshUvs; 

		protected override void Awake ()
		{
			var particleSystem = GetComponent<ParticleSystem> ();
			var particleSystemRenderer = GetComponent<ParticleSystemRenderer> ();
			if (m_Material == null) {
				m_Material = particleSystemRenderer.sharedMaterial;
			}
		    if(particleSystemRenderer.renderMode == ParticleSystemRenderMode.Stretch)
		        RenderMode = UiParticleRenderMode.StreachedBillboard;
			
			base.Awake ();
			ParticleSystem = particleSystem;
			m_ParticleSystemRenderer = particleSystemRenderer;
			InitMeshData();
		}

		private void InitMeshData()
		{
			if (RenderedMesh != null && RenderedMesh != _cachedMesh)
			{
				m_MeshVerts = RenderedMesh.vertices;
				m_MeshTriangles = RenderedMesh.triangles;
				m_MeshUvs = RenderedMesh.uv;
				_cachedMesh = RenderedMesh;
			}
		}


		public override void SetMaterialDirty ()
		{
			base.SetMaterialDirty ();
			if (m_ParticleSystemRenderer != null)
				m_ParticleSystemRenderer.sharedMaterial = m_Material;
		}

		protected override void UpdateGeometry()
		{
			var mesh = workerMesh;
			_geometry.Reset();
			if (ParticleSystem != null && ParticleSystem.isPlaying && ParticleSystem.particleCount != 0)
			{
				GenerateParticlesBillboards();
				if (_geometry.Expose(mesh))
				{
					canvasRenderer.SetMesh(mesh);
				}
				else
				{
					workerMesh.Clear();
					canvasRenderer.SetMesh(mesh);
				}
			}
			else
			{
				mesh.Clear();
				canvasRenderer.SetMesh(mesh);
			}
		}

		protected override void OnDestroy()
		{
			_geometry.Dispose();
		}
		
		protected virtual void Update ()
		{
			if (!m_IgnoreTimescale)
			{
				if (ParticleSystem != null && ParticleSystem.isPlaying)
				{
					SetVerticesDirty();
				}
				else if(!_geometry.IsEmpty)
				{
					_geometry.Reset();
					SetVerticesDirty();
				}
			}
			else
			{
				if (ParticleSystem != null)
				{
					ParticleSystem.Simulate(Time.unscaledDeltaTime, true, false);
					SetVerticesDirty();
				}
				else if(!_geometry.IsEmpty)
				{
					_geometry.Reset();
					SetVerticesDirty();
				}
			}

			// disable default particle renderer, we using our custom
			if (m_ParticleSystemRenderer != null && m_ParticleSystemRenderer.enabled)
				m_ParticleSystemRenderer.enabled = false;
		}
			

		private void InitParticlesBuffer (ParticleSystem.MainModule mainModule)
		{
			if (m_Particles == null || m_Particles.Length < mainModule.maxParticles)
			{
				m_Particles = new ParticleSystem.Particle[mainModule.maxParticles];
			}
			
		}
	
		private void GenerateParticlesBillboards ()
		{
			//read modules ones, cause they produce allocations when read.
			var mainModule = ParticleSystem.main;
			
			var textureSheetAnimationModule = ParticleSystem.textureSheetAnimation;
			
			InitParticlesBuffer (mainModule);
			int numParticlesAlive = ParticleSystem.GetParticles (m_Particles);
			
			
			//!NOTE sample curves before render particles, because it produces allocations
			var frameOverTime = ParticleSystem.textureSheetAnimation.frameOverTime;
			var velocityOverLifeTime = ParticleSystem.velocityOverLifetime;
			var velocityOverTimeX = velocityOverLifeTime.x;
			var velocityOverTimeY = velocityOverLifeTime.y;
			var velocityOverTimeZ = velocityOverLifeTime.z;
			var isWorldSimulationSpace = mainModule.simulationSpace == ParticleSystemSimulationSpace.World;

			if (RenderMode == UiParticleRenderMode.Mesh)
			{
				if (RenderedMesh != null)
				{
					InitMeshData();
					_geometry.SetVerticesCount(m_MeshVerts.Length*numParticlesAlive);
					_geometry.SetIndicesCount(m_MeshTriangles.Length*numParticlesAlive);
					for (int i = 0; i < numParticlesAlive; i++)
					{
						DrawParticleMesh(m_Particles[i], i, frameOverTime, isWorldSimulationSpace,
							textureSheetAnimationModule, m_MeshVerts, m_MeshTriangles, m_MeshUvs);
					}
				}
			}
			else
			{
				_geometry.SetVerticesCount(4*numParticlesAlive);
				_geometry.SetIndicesCount(6*numParticlesAlive);
				for (int i = 0; i < numParticlesAlive; i++) 
				{
					DrawParticleBillboard (m_Particles [i], i, frameOverTime, 
						velocityOverTimeX, velocityOverTimeY, velocityOverTimeZ, isWorldSimulationSpace, 
						textureSheetAnimationModule);
				}
			}
		}

		private void DrawParticleMesh(
			ParticleSystem.Particle particle,
			int particleIndex,
			ParticleSystem.MinMaxCurve frameOverTime,
			bool isWorldSimulationSpace,
			ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule, Vector3[] verts, int[] triangles,
			Vector2[] uvs)
		{
			var center =  particle.position; 
			var rotation = Quaternion.Euler (particle.rotation3D);

			if (isWorldSimulationSpace)
			{
				center = rectTransform.InverseTransformPoint (center);
			}

			float timeAlive = particle.startLifetime - particle.remainingLifetime;

			Vector3 size3D = particle.GetCurrentSize3D (ParticleSystem);
			Color32 color32 = particle.GetCurrentColor (ParticleSystem);

			CalculateUvs(particle, frameOverTime, textureSheetAnimationModule, timeAlive, out var uv0, out _, out var uv2, out _);

			var vertOffset = particleIndex * verts.Length;
			
			var positions = _geometry.Positions;
			var colors = _geometry.Colors;
			var finalUvs = _geometry.Uvs;
			
			for (int j = 0; j < verts.Length; j++)
			{
				Vector3 pos = verts[j];
				pos.x *= size3D.x;
				pos.y *= size3D.y;
				pos.z *= size3D.z;
				pos = rotation * pos + center;

				var uvXpercent = uvs[j].x;
				var uvYpercent = uvs[j].y;

				var newUvx = Mathf.Lerp(uv0.x, uv2.x, uvXpercent);
				var newUvy = Mathf.Lerp(uv0.y, uv2.y, uvYpercent);

				positions[vertOffset + j] = pos;
				colors[vertOffset + j] = color32;
				finalUvs[vertOffset + j] = new Vector2(newUvx, newUvy);
			}
			
			var indices = _geometry.Indices;
			var indicesOffset = triangles.Length * particleIndex;
			
			for (int i = 0; i < triangles.Length; i++)
			{
				indices[indicesOffset + i] = (ushort) (vertOffset + triangles[i]);
			}
		}
		

		private void DrawParticleBillboard (
			ParticleSystem.Particle particle,
			int particleIndex,
			ParticleSystem.MinMaxCurve frameOverTime, 
			ParticleSystem.MinMaxCurve velocityOverTimeX, 
			ParticleSystem.MinMaxCurve velocityOverTimeY, 
			ParticleSystem.MinMaxCurve velocityOverTimeZ,
			bool isWorldSimulationSpace,
			ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule)
		{
			var center =  particle.position; 
			var rotation = Quaternion.Euler (particle.rotation3D);

			if (isWorldSimulationSpace)
			{
				center = rectTransform.InverseTransformPoint (center);
			}

			float timeAlive = particle.startLifetime - particle.remainingLifetime;
			float globalTimeAlive = timeAlive / particle.startLifetime;

			Vector3 size3D = particle.GetCurrentSize3D (ParticleSystem);
			
			if(m_RenderMode == UiParticleRenderMode.StreachedBillboard)
			{
				GetStrechedBillboardsSizeAndRotation(particle,globalTimeAlive,ref size3D, out rotation,
					velocityOverTimeX, velocityOverTimeY, velocityOverTimeZ);
			}

			var leftTop = new Vector3 (-size3D.x * 0.5f, size3D.y * 0.5f);
			var rightTop = new Vector3 (size3D.x * 0.5f, size3D.y * 0.5f);
			var rightBottom = new Vector3 (size3D.x * 0.5f, -size3D.y * 0.5f);
			var leftBottom = new Vector3 (-size3D.x * 0.5f, -size3D.y * 0.5f);


			leftTop = rotation * leftTop + center;
			rightTop = rotation * rightTop + center;
			rightBottom = rotation * rightBottom + center;
			leftBottom = rotation * leftBottom + center;

			Color32 color32 = particle.GetCurrentColor (ParticleSystem);

			CalculateUvs(particle, frameOverTime, textureSheetAnimationModule, timeAlive, out var uv0, out var uv1, out var uv2, out var uv3);

			var vertOffset = particleIndex * 4;
			
			var positions = _geometry.Positions;
			positions[vertOffset] = leftBottom;
			positions[vertOffset+1] = leftTop;
			positions[vertOffset+2] = rightTop;
			positions[vertOffset+3] = rightBottom;

			var uvs = _geometry.Uvs;
			uvs[vertOffset] = uv0;
			uvs[vertOffset+1] = uv1;
			uvs[vertOffset+2] = uv2;
			uvs[vertOffset+3] = uv3;

			var colors = _geometry.Colors;
			colors[vertOffset] = color32;
			colors[vertOffset+1] = color32;
			colors[vertOffset+2] = color32;
			colors[vertOffset+3] = color32;

			var indOffset = particleIndex * 6;
			var indices = _geometry.Indices;
			indices[indOffset] =  (ushort) vertOffset;
			indices[indOffset+1] = (ushort) (vertOffset+1);
			indices[indOffset+2] = (ushort) (vertOffset+2);
			indices[indOffset+3] = (ushort) (vertOffset+2);
			indices[indOffset+4] = (ushort) (vertOffset+3);
			indices[indOffset+5] = (ushort) (vertOffset+0);

		}

		private static void CalculateUvs(ParticleSystem.Particle particle, ParticleSystem.MinMaxCurve frameOverTime,
			ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule, 
			float timeAlive, out Vector2 uv0, out Vector2 uv1, out Vector2 uv2,
			out Vector2 uv3)
		{
			if (!textureSheetAnimationModule.enabled)
			{
				uv0 = new Vector2(0f, 0f);
				uv1 = new Vector2(0f, 1f);
				uv2 = new Vector2(1f, 1f);
				uv3 = new Vector2(1f, 0f);
			}
			else
			{
				float lifeTimePerCycle = particle.startLifetime / textureSheetAnimationModule.cycleCount;
				float timePerCycle = timeAlive % lifeTimePerCycle;
				float timeAliveAnim01 = timePerCycle / lifeTimePerCycle; // in percents


				var totalFramesCount = textureSheetAnimationModule.numTilesY * textureSheetAnimationModule.numTilesX;
				var frame01 = frameOverTime.Evaluate(timeAliveAnim01);

				var frame = 0f;
				switch (textureSheetAnimationModule.animation)
				{
					case ParticleSystemAnimationType.WholeSheet:
					{
						frame = Mathf.Clamp(Mathf.Floor(frame01 * totalFramesCount), 0, totalFramesCount - 1);
						break;
					}
					case ParticleSystemAnimationType.SingleRow:
					{
						frame = Mathf.Clamp(Mathf.Floor(frame01 * textureSheetAnimationModule.numTilesX), 0,
							textureSheetAnimationModule.numTilesX - 1);
						int row = textureSheetAnimationModule.rowIndex;
						if (textureSheetAnimationModule.rowMode == ParticleSystemAnimationRowMode.Random)
						{
							Random.InitState((int) particle.randomSeed);
							row = Random.Range(0, textureSheetAnimationModule.numTilesY);
						}
						frame += row * textureSheetAnimationModule.numTilesX;
						break;
					}
				}

				int x = (int) frame % textureSheetAnimationModule.numTilesX;
				int y = (int) frame / textureSheetAnimationModule.numTilesX;

				var xDelta = 1f / textureSheetAnimationModule.numTilesX;
				var yDelta = 1f / textureSheetAnimationModule.numTilesY;
				y = textureSheetAnimationModule.numTilesY - 1 - y;
				var sX = x * xDelta;
				var sY = y * yDelta;
				var eX = sX + xDelta;
				var eY = sY + yDelta;

				uv0 = new Vector2(sX, sY);
				uv1 = new Vector2(sX, eY);
				uv2 = new Vector2(eX, eY);
				uv3 = new Vector2(eX, sY);
			}
		}


		/// <summary>
		/// Evaluate size and roatation of particle in streched billboard mode
		/// </summary>
		/// <param name="particle">particle</param>
		/// <param name="timeAlive01">current life time percent [0,1] range</param>
		/// <param name="size3D">particle size</param>
		/// <param name="rotation">particle rotation</param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		private void GetStrechedBillboardsSizeAndRotation(ParticleSystem.Particle particle, float timeAlive01,
			ref Vector3 size3D, out Quaternion rotation, 
			ParticleSystem.MinMaxCurve x, ParticleSystem.MinMaxCurve y, ParticleSystem.MinMaxCurve z)
		{
			var velocityOverLifeTime = Vector3.zero;

			if (ParticleSystem.velocityOverLifetime.enabled)
			{
				velocityOverLifeTime.x = x.Evaluate(timeAlive01);
				velocityOverLifeTime.y = y.Evaluate(timeAlive01);
				velocityOverLifeTime.z = z.Evaluate(timeAlive01);
			}
		    
			var finalVelocity = particle.velocity + velocityOverLifeTime;
			var ang = Vector3.Angle(finalVelocity,  Vector3.up);
			var horizontalDirection = finalVelocity.x < 0 ? 1 : -1;
			rotation = Quaternion.Euler(new Vector3(0,0, ang*horizontalDirection));
			size3D.y *=m_StretchedLenghScale;
			size3D+= new Vector3(0, m_StretchedSpeedScale*finalVelocity.magnitude);
		}
		
		private struct Geometry
		{
			private NativeArray<Vector3> _positions;
			private NativeArray<Vector2> _uvs;
			private NativeArray<Color32> _colors;
			private NativeArray<Vector2> _secondaryUvs;
			private NativeArray<Vector4> _tangents;
			private NativeArray<Vector3> _normals;
			private NativeArray<ushort> _indices;

			public bool IsEmpty => _verticesCount == 0;
			private int _verticesCount;
			private int _indicesCount;
			
			public NativeSlice<Vector3> Positions => _positions.Slice(0, _verticesCount);
			public NativeSlice<Vector2> Uvs => _uvs.Slice(0, _verticesCount);
			public NativeSlice<ushort> Indices => _indices.Slice(0, _indicesCount);
			public NativeSlice<Color32> Colors => _colors.Slice(0, _verticesCount);


			public void Reset()
			{
				_verticesCount = 0;
				_indicesCount = 0;
			}

			public void SetVerticesCount(int vertCount)
			{
				_verticesCount = vertCount;
				if (_positions.Length >= vertCount)
					return;

				DisposeVertices();
				var capacity = NextPow2(vertCount);
				_positions = new NativeArray<Vector3>(capacity, Allocator.Persistent);
				_colors = new NativeArray<Color32>(capacity, Allocator.Persistent);
				_uvs = new NativeArray<Vector2>(capacity, Allocator.Persistent);
				_secondaryUvs = new NativeArray<Vector2>(capacity, Allocator.Persistent);
				_tangents = new NativeArray<Vector4>(capacity, Allocator.Persistent);
				_normals = new NativeArray<Vector3>(capacity, Allocator.Persistent);

				Fill(ref _secondaryUvs, vertCount, Vector2.zero);
				Fill(ref _tangents, vertCount, new Vector4(1.0f, 0.0f, 0.0f, -1.0f));
				Fill(ref _normals, vertCount, Vector3.back);
			}

			public void SetIndicesCount(int indCount)
			{
				_indicesCount = indCount;
				var capacity = NextPow2(indCount);
				_indices = new NativeArray<ushort>(capacity, Allocator.Persistent);
			}

			public void Dispose()
			{
				DisposeVertices();
				DisposeIndices();
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void DisposeVertices()
			{
				if (!_positions.IsCreated)
					return;

				_positions.Dispose();
				_colors.Dispose();
				_uvs.Dispose();
				_secondaryUvs.Dispose();
				_tangents.Dispose();
				_normals.Dispose();
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void DisposeIndices()
			{
				if (_indices.IsCreated)
					_indices.Dispose();
			}

			public bool Expose(Mesh mesh)
			{
				if(!_positions.IsCreated)
					return false;
				mesh.Clear();
				mesh.SetVertices(_positions, 0, _verticesCount);
				mesh.SetColors(_colors, 0, _verticesCount);
				mesh.SetUVs(0, _uvs, 0, _verticesCount);
				mesh.SetUVs(1, _secondaryUvs, 0, _verticesCount);
				mesh.SetUVs(2, _secondaryUvs, 0, _verticesCount);
				mesh.SetUVs(3, _secondaryUvs, 0, _verticesCount);
				mesh.SetNormals(_normals, 0, _verticesCount);
				mesh.SetTangents(_tangents, 0, _verticesCount);
				mesh.SetIndices(_indices, 0, _indicesCount, MeshTopology.Triangles, 0);
				return true;
			}

			private static void Fill<T>(ref NativeArray<T> slice, int count, T value)
				where T : unmanaged
			{
				var items = slice;
				for (var i = 0; i < count; ++i)
					items[i] = value;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int NextPow2(int i)
			{
				--i;
				i |= i >> 1;
				i |= i >> 2;
				i |= i >> 4;
				i |= i >> 8;
				i |= i >> 16;
				return i + 1;
			}
		}
	}


	/// <summary>
	/// Particles Render Modes
	/// </summary>
    public enum UiParticleRenderMode
    {
        Billboard,
        StreachedBillboard,
	    Mesh
    }
}
