# ---- name signs on buildings (OSM names only)
def _nuv(n):
    i=SROW[n]; H=_Im.open('/workspace/ValVertGame/Assets/ValVert/Gen/sign_names.png').size[1] if False else _AH; u1=SASP[n][0]
    v0=1-(i+1)*_RH/_AH; v1=1-i*_RH/_AH; return [(0,v0+.002),(u1,v0+.002),(u1,v1-.002),(0,v1-.002)]
def _sign(n,p,g,yt,kind,target,small=False):
    r=[tuple(q) for q in p]
    if not small:
        # ensure CCW for facing()
        A=sum(r[i][0]*r[(i+1)%len(r)][1]-r[(i+1)%len(r)][0]*r[i][1] for i in range(len(r)))
        if A<0: r=r[::-1]
    a,b,nn,L=facing(r,target); a=np.array(a); b=np.array(b); d=(b-a)/L; nn=np.array(nn); m=(a+b)/2
    if np.dot(nn,np.array(target)-m)<0: nn=-nn
    asp=SASP[n][1]
    if small: hh=0.35
    elif kind=='comm': hh=min(1.6,max(0.8,(L*0.7)/asp))
    else: hh=min(0.6,(L*0.8)/asp)
    ww=hh*asp
    if ww>L*0.9: ww=L*0.9; hh=ww/asp
    ys=(yt-hh-0.6) if kind=='comm' and not small else (g+0.3+2.0 if small else Hc(*m)+2.7)
    if small: ys=Hc(*m)+1.8
    yaw=math.atan2(d[1],d[0]); bq=m+nn*0.08; b_box('coping',(bq[0],ys-0.05,bq[1]),(ww+0.1,hh+0.1,0.12),yaw)
    q0=m-d*ww/2+nn*0.16; q1=m+d*ww/2+nn*0.16
    b_quad('sign_names',(q0[0],ys,q0[1]),(q1[0],ys,q1[1]),(q1[0],ys+hh,q1[1]),(q0[0],ys+hh,q0[1]),_nuv(n),np.array([nn[0],0,nn[1]]))
    aud('sign:name')
    if n in('GiFi','Armataffet','Giusti et Fils Ets'): print('SIGNPOS',n,np.round(m,1),np.round(nn,2),round(ys,1),round(hh,2),round(ww,1),kind)
def _yt(p,g,yb,kind,top):
    if kind=='comm': return max(yb+3.5,g+0.25+min(top,12))
    return g+min(top,12)
def _roadt(c):
    best=None
    for e,hw,pts in ROADS:
        for q in pts:
            dd=np.hypot(*(q-c))
            if best is None or dd<best[0]: best=(dd,q)
    return best[1] if best else c+np.array([0,10])
_cnt=0
for wid,n in NAMEDW.items():
    if wid not in BI: continue
    p,g,yb,kind,top=BI[wid]; c=np.array(p).mean(0); small=(kind=='small' or n in('Barral P2','Sainte-Catherine','Daviet','Vendome','Menu'))
    _sign(n,p,g,_yt(p,g,yb,kind,top),'comm' if kind=='comm' else kind,_roadt(c),small); _cnt+=1
for n,q in NAMEDN:
    best=None
    for wid,(p,g,yb,kind,top) in BI.items():
        c=np.array(p).mean(0)
        if abs(c[0]-q[0])>80 or abs(c[1]-q[1])>80: continue
        if _Path(np.array(p)).contains_point(tuple(q),radius=0.5): best=wid; break
        dd=np.min(np.hypot(np.array(p)[:,0]-q[0],np.array(p)[:,1]-q[1]))
        if dd<12 and (best is None or dd<best[1]): best=(wid,dd)
    if best is None: aud('sign:name_nobuilding'); continue
    wid=best if not isinstance(best,tuple) else best[0]; p,g,yb,kind,top=BI[wid]
    _sign(n,p,g,_yt(p,g,yb,kind,top),kind,q+(q-np.array(p).mean(0))*3+np.array([0.01,0.01]) if np.hypot(*(q-np.array(p).mean(0)))>1 else _roadt(q)); _cnt+=1
print('named signs',_cnt)
