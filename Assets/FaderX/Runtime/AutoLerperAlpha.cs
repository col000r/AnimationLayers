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
    public class AutoLerperAlpha : MonoBehaviour {
        [SerializeField] private FloatLerper m_Lerper = new FloatLerper();
        [SerializeField] private Image m_Image;
        [SerializeField] private CanvasGroup m_CanvasGroup;

        public float CurrentValue => m_Lerper.CurrentValue;

        private void Awake() {
            if(m_Lerper == null) m_Lerper = new FloatLerper();
        }

        public void Show() {
            m_Lerper.LerpTo( 1f );
        }

        public void Hide() {
            m_Lerper.LerpTo( 0f );
        }

        public void LerpTo( float f ) {
            m_Lerper.LerpTo( f );
        }
        
        private void Update() {
            if( m_Image != null ) m_Image.color = new Color( m_Image.color.r, m_Image.color.g, m_Image.color.b, m_Lerper.CurrentValue );
            if( m_CanvasGroup != null ) m_CanvasGroup.alpha = m_Lerper.CurrentValue;
        }
    }
}