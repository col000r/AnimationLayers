// Copyright (c) 2020 Markus Hofer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#pragma warning disable 0649
using UnityEngine;
using UnityEngine.UI;

namespace FaderX {
    public class AutoLerperColor : MonoBehaviour {

        [SerializeField] private bool m_LerpColor = true;
        [SerializeField] private bool m_LerpLocalScale = true;
        
        [SerializeField] private bool m_AutoStart;
        [SerializeField] private Color m_AutoStartColor;
        [SerializeField] private Color m_AutoTargetColor;
        [SerializeField] private float m_AutoLerpSpeed = 5f;

        [SerializeField] private float m_LerpSpeed = 5f;
        
        [SerializeField] private ColorLerper m_ColorLerper = new ColorLerper();

        [SerializeField] private Image m_Image;
        [SerializeField] private CanvasGroup m_CanvasGroup;
        [SerializeField] private RectTransform m_RectTransform;

        [SerializeField] private float m_ScaleMin = 0f;
        [SerializeField] private float m_ScaleMax = 1f;

        public Color CurrentValue => m_ColorLerper.CurrentValue;

        private void Start() {
            if( m_AutoStart ) {
                m_ColorLerper.JumpTo( m_AutoStartColor );
                m_ColorLerper.LerpTo( m_AutoTargetColor, m_AutoLerpSpeed );
            }
        }

        public void LerpTo( Color color ) {
            m_ColorLerper.LerpTo( color, m_LerpSpeed );
        }

        private void Update() {
            if( !m_ColorLerper.Done ) {
                if( m_LerpColor ) {
                    if( m_Image != null ) m_Image.color = m_ColorLerper.CurrentValue;
                    if( m_CanvasGroup != null ) m_CanvasGroup.alpha = m_ColorLerper.CurrentValue.a;
                }

                if( m_LerpLocalScale ) {
                    if( m_RectTransform != null ) m_RectTransform.localScale = Vector3.one * Mathf.Lerp( m_ScaleMin, m_ScaleMax, m_ColorLerper.CurrentValue.a );
                }
            }
        }

        public void LerpFromTo( Color fromColor, Color toColor ) {
            m_ColorLerper.JumpTo( fromColor );
            m_ColorLerper.LerpTo( toColor );
        }
    }
}