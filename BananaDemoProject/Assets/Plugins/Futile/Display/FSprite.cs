using UnityEngine;
using System;

public class FSprite : FQuadNode
{
	protected Color _color = Color.white;
	protected Color _alphaColor = Color.white;
	
	protected Vector2[] _localVertices = new Vector2[4];
	
	protected float _anchorX = 0.5f;
	protected float _anchorY = 0.5f;
	
	protected Rect _localRect;

	protected bool _isMeshDirty = false;
	protected bool _areLocalVerticesDirty = false;
	
	public FSprite (string elementName) : base()
	{
		//_element = _layer.renderer.GetElementWithName(elementName);
		
		Init (FEngine.atlasManager.GetElementWithName(elementName),1);
		
		_isAlphaDirty = true;
		
		UpdateLocalVertices();
	}
	
	public void SetElementByName(string elementName)
	{
		this.element = FEngine.atlasManager.GetElementWithName(elementName);
	}
	
	override public void HandleElementChanged()
	{
		_areLocalVerticesDirty = true;
	}
	
	override public void Update(bool shouldForceDirty, bool shouldUpdateDepth)
	{
		bool wasMatrixDirty = _isMatrixDirty;
		bool wasAlphaDirty = _isAlphaDirty;
		
		UpdateDepthMatrixAlpha(shouldForceDirty, shouldUpdateDepth);
		
		if(shouldUpdateDepth)
		{
			UpdateQuads();
		}
		
		if(wasMatrixDirty || shouldForceDirty || shouldUpdateDepth)
		{
			_isMeshDirty = true;
		}
		
		if(wasAlphaDirty || shouldForceDirty)
		{
			_isMeshDirty = true;
			_alphaColor = _color.CloneWithMultipliedAlpha(_concatenatedAlpha);	
		}
		
		if(_areLocalVerticesDirty)
		{
			UpdateLocalVertices();
		}
		
		//only populate the render layer if it's NOT a depth update
		//because if it IS a depth update, populate will be called LATER
		if(_isMeshDirty) 
		{
			PopulateRenderLayer();
		}
	}
	
	virtual protected void UpdateLocalVertices()
	{
		_areLocalVerticesDirty = false;
		
		float sourceWidth = _element.sourceRect.width;
		float sourceHeight = _element.sourceRect.height;
		float left = -_anchorX*sourceWidth;
		float bottom = -_anchorY*sourceHeight;
		
		//Debug.Log ("sourceSize: " + sourceWidth + "," + sourceHeight + "   left: " + left + "   bottom: " + bottom);
		
		_localRect.x = left;
		_localRect.y = bottom;
		_localRect.width = sourceWidth;
		_localRect.height = sourceHeight;
		
		_localVertices[0].Set(left,bottom + sourceHeight);
		_localVertices[1].Set(left + sourceWidth,bottom + sourceHeight);
		_localVertices[2].Set(left + sourceWidth,bottom);
		_localVertices[3].Set(left,bottom);
		
		_isMeshDirty = true;
		
		//RXUtils.LogVectors(_localVertices[0],_localVertices[1],_localVertices[2],_localVertices[3]);
	}
	
	override public void PopulateRenderLayer()
	{
		if(_isOnStage && _firstQuadIndex != -1) 
		{
			_isMeshDirty = false;
			
			int vertexIndex = _firstQuadIndex*4;
			
			Vector3[] vertices = _renderLayer.vertices;
			Vector2[] uvs = _renderLayer.uvs;
			Color[] colors = _renderLayer.colors;
			
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex + 0], _localVertices[0],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex + 1], _localVertices[1],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex + 2], _localVertices[2],0);
			_concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[vertexIndex + 3], _localVertices[3],0);
			
			uvs[vertexIndex + 0] = _element.uvTopLeft;
			uvs[vertexIndex + 1] = _element.uvTopRight;
			uvs[vertexIndex + 2] = _element.uvBottomRight;
			uvs[vertexIndex + 3] = _element.uvBottomLeft;
			
			colors[vertexIndex + 0] = _alphaColor;
			colors[vertexIndex + 1] = _alphaColor;
			colors[vertexIndex + 2] = _alphaColor;
			colors[vertexIndex + 3] = _alphaColor;
			
			_renderLayer.HandleVertsChange();
		}
	}
	
	virtual public Rect localRect
	{
		get {return _localRect;}	
	}

	virtual public Color color 
	{
		get { return _color; }
		set 
		{ 
			if(_color != value)
			{
				_color = value; 
				_isAlphaDirty = true;
			}
		}
	}
	
	virtual public float width
	{
		get { return _scaleX * _localRect.width; }
		set { _scaleX = value/_localRect.width; _isMatrixDirty = true; } 
	}
	
	virtual public float height
	{
		get { return _scaleY * _localRect.height; }
		set { _scaleY = value/_localRect.height; _isMatrixDirty = true; } 
	}
	
	virtual public float anchorX 
	{
		get { return _anchorX;}
		set { _anchorX = value; _areLocalVerticesDirty = true; }
	}
	
	virtual public float anchorY 
	{
		get { return _anchorY;}
		set { _anchorY = value; _areLocalVerticesDirty = true; }
	}
}

