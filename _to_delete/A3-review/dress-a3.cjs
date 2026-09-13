const fs = require('fs');
const path = require('path');
const scenePath = 'Do not leave me/Assets/DoNotLeaveMe/Levels/Level_04A 3.unity';
const parse = s => [...s.matchAll(/^--- !u!(\d+) &(\d+)[^\n]*\n([\s\S]*?)(?=^--- !u!|$(?![\s\S]))/gm)].map(m => ({type:m[1],id:m[2],body:m[3],raw:m[0]}));
const source = fs.readFileSync(scenePath,'utf8'), docs = parse(source), map = new Map(docs.map(d=>[d.id,d]));
if (source.includes('A3_A1_Decoration_DetailPass')) throw Error('Detail pass already present');
const field=(d,k)=>d.body.match(new RegExp(k+': \\{fileID: (\\d+)'))?.[1];
const vector=(d,k)=>Object.fromEntries([...d.body.match(new RegExp(k+': \\{([^}]+)'))[1].matchAll(/([xyzw]): ([^,}]+)/g)].map(m=>[m[1],Number(m[2])]));
const goName=d=>map.get(field(d,'m_GameObject'))?.body.match(/m_Name: (.*)/)?.[1]||'';
const transforms=docs.filter(d=>d.type==='4'&&!d.raw.includes('stripped'));
const named=n=>transforms.find(d=>goName(d)===n);
const assets=new Map();
function scan(p){for(const e of fs.readdirSync(p,{withFileTypes:true})){const q=path.join(p,e.name);if(e.isDirectory())scan(q);else if(q.endsWith('.meta')){let guid=fs.readFileSync(q,'utf8').match(/^guid: (\w+)/m)?.[1];if(guid)assets.set(guid,q.slice(0,-5));}}}
scan('Do not leave me/Assets');
const add=(a,b)=>({x:a.x+b.x,y:a.y+b.y,z:a.z+b.z});
const mul=(a,s)=>({x:a.x*s,y:a.y*s,z:a.z*s});
function rotate(v,q){const u={x:q.x,y:q.y,z:q.z};const cross=(a,b)=>({x:a.y*b.z-a.z*b.y,y:a.z*b.x-a.x*b.z,z:a.x*b.y-a.y*b.x});return add(v,add(mul(cross(u,v),2*q.w),mul(cross(u,cross(u,v)),2)));}
function subtree(root){const ids=new Set([root]);let change=true;while(change){change=false;for(const d of docs){if(ids.has(d.id))continue;const parent=field(d,'m_Father')||field(d,'m_TransformParent'),go=field(d,'m_GameObject'),pi=field(d,'m_PrefabInstance');if(parent&&ids.has(parent)||go&&ids.has(go)||pi&&ids.has(pi)){ids.add(d.id);if(go&&go!=='0')ids.add(go);change=true;}}if(map.has(root)){const go=field(map.get(root),'m_GameObject');if(go&&!ids.has(go)){ids.add(go);change=true;}}}return docs.filter(d=>ids.has(d.id));}
function cornersFor(root){let sub=subtree(root),pts=[];for(const d of sub.filter(d=>d.type==='33')){const m=d.raw.match(/m_Mesh: \{fileID: [^,]+, guid: (\w+)/);if(!m)throw Error('No mesh');const file=assets.get(m[1]);const text=fs.readFileSync(file,'utf8');const box=text.match(/m_LocalAABB:\s*\n\s*m_Center: \{([^}]+)\}\s*\n\s*m_Extent: \{([^}]+)\}/);if(!box)throw Error('No mesh bounds: '+file);const v=s=>Object.fromEntries([...s.matchAll(/([xyz]): ([^,}]+)/g)].map(m=>[m[1],Number(m[2])]));const c=v(box[1]),e=v(box[2]);let t=transforms.find(t=>field(t,'m_GameObject')===field(d,'m_GameObject'));for(let x of [-1,1])for(let y of [-1,1])for(let z of [-1,1]){let point={x:c.x+x*e.x,y:c.y+y*e.y,z:c.z+z*e.z},node=t;while(node.id!==root){let s=vector(node,'m_LocalScale');point=add(rotate({x:point.x*s.x,y:point.y*s.y,z:point.z*s.z},vector(node,'m_LocalRotation')),vector(node,'m_LocalPosition'));node=map.get(field(node,'m_Father'));}pts.push(point);}}return pts;}
let counter=12000000000n;const fresh=()=>String(counter++);
const detailGo=fresh(),detailTransform=fresh(),art=named('A3_Art_RebuiltAlongRoute');
const appended=[],roots=[],audit=[];
const templates=['1834295377','294207807','842982706','839062913','63060951','1961365212','1969428056','2053864697'].map(id=>({id,doc:map.get(id),parts:subtree(id),corners:cornersFor(id)}));
function clone(template,position,q,scale,name){const ids=new Map(template.parts.map(d=>[d.id,fresh()]));for(const d of template.parts){let raw=d.raw.replace(/^--- !u!(\d+) &(\d+)/,(_,t,id)=>'--- !u!'+t+' &'+ids.get(id)).replace(/\{fileID: (\d+)\}/g,(all,id)=>ids.has(id)?'{fileID: '+ids.get(id)+'}':all);if(d.id===template.id){raw=raw.replace(/m_LocalPosition: \{[^}]+\}/,'m_LocalPosition: {x: '+position.x+', y: '+position.y+', z: '+position.z+'}').replace(/m_LocalRotation: \{[^}]+\}/,'m_LocalRotation: {x: '+q.x+', y: '+q.y+', z: '+q.z+', w: '+q.w+'}').replace(/m_LocalScale: \{[^}]+\}/,'m_LocalScale: {x: '+scale+', y: '+scale+', z: '+scale+'}').replace(/m_Father: \{fileID: \d+\}/,'m_Father: {fileID: '+detailTransform+'}');}if(d.id===field(template.doc,'m_GameObject'))raw=raw.replace(/m_Name: .*/,'m_Name: '+name);appended.push(raw);}roots.push(ids.get(template.id));}
const points=['A3_AdmissionStart','A3_AdmissionTurn','SegmentPoint_0','SegmentPoint_1','SegmentPoint_2','SegmentPoint_3','SegmentPoint_4'].map(n=>vector(named(n),'m_LocalPosition'));
for(let i=0;i<6;i++){
 const start=points[i],end=points[i+1],delta=add(end,mul(start,-1)),length=Math.hypot(delta.x,delta.z),f=mul(delta,1/length),r={x:f.z,y:0,z:-f.x};
 const existing=transforms.filter(d=>goName(d).startsWith('A1_Prop_')).map(d=>vector(d,'m_LocalPosition')).map(p=>({along:(p.x-start.x)*f.x+(p.z-start.z)*f.z,side:(p.x-start.x)*r.x+(p.z-start.z)*r.z}));
 for(let side of [-1,1])for(let distance=7.5;distance<length-5;distance+=3.8){
  if(existing.some(p=>Math.abs(p.side-side*4.05)<.5&&Math.abs(p.along-distance)<2.3))continue;
  const index=(i*3+Math.round(distance)+(side===1?2:0))%templates.length,t=templates[index];
  const yaw=Math.atan2(f.x,f.z)+side*Math.PI/2+((index%3)-1)*.07,q={x:0,y:Math.sin(yaw/2),z:0,w:Math.cos(yaw/2)};
  let scale=vector(t.doc,'m_LocalScale').x*1.08,rot=t.corners.map(p=>rotate(p,q));
  const lateral=rot.map(p=>p.x*r.x+p.z*r.z),lo=Math.min(...lateral),hi=Math.max(...lateral);
  scale=Math.min(scale,1.45/(hi-lo));
  const pos=add(add(start,mul(f,distance)),mul(r,side*4.12));pos.y=start.y-Math.min(...rot.map(p=>p.y))*scale;
  const inner=side===1?4.12+lo*scale:4.12-hi*scale,outer=side===1?4.12+hi*scale:4.12-lo*scale;
  if(inner<3.25||outer>5.02)throw Error('Prop crosses lane or wall: '+inner+' '+outer);
  clone(t,pos,q,scale,'A1_Dressing_Hall'+(i+1)+'_'+(side===1?'R':'L')+'_'+distance.toFixed(1));
  audit.push({hall:i+1,asset:goName(map.get(field(t.parts.find(d=>d.type==='33'),'m_GameObject')))||t.id,inner,outer});
 }
}
// Repeat A1's small wall fixtures and hospital signage at readable intervals.
const decorativeNames=['A1_HospitalSign','A1_WallLamp'];let accents=0;
for(const name of decorativeNames){const t=transforms.find(d=>goName(d)===name);if(!t)continue;let template={id:t.id,doc:t,parts:subtree(t.id)};for(let i=0;i<6;i++){const start=points[i],end=points[i+1],d=add(end,mul(start,-1)),len=Math.hypot(d.x,d.z),f=mul(d,1/len),r={x:f.z,y:0,z:-f.x};for(const side of [-1,1])for(let at=10;at<len-5;at+=12){const p=add(add(start,mul(f,at)),mul(r,side*(name.includes('Sign')?4.85:4.7)));p.y=name.includes('Sign')?2.75:3.15;const yaw=Math.atan2(f.x,f.z)+side*Math.PI/2,q={x:0,y:Math.sin(yaw/2),z:0,w:Math.cos(yaw/2)};const ids=new Map(template.parts.map(d=>[d.id,fresh()]));for(const doc of template.parts){let raw=doc.raw.replace(/^--- !u!(\d+) &(\d+)/,(_,ty,id)=>'--- !u!'+ty+' &'+ids.get(id)).replace(/\{fileID: (\d+)\}/g,(all,id)=>ids.has(id)?'{fileID: '+ids.get(id)+'}':all);if(doc.id===t.id)raw=raw.replace(/m_LocalPosition: \{[^}]+\}/,`m_LocalPosition: {x: ${p.x}, y: ${p.y}, z: ${p.z}}`).replace(/m_LocalRotation: \{[^}]+\}/,`m_LocalRotation: {x: 0, y: ${q.y}, z: 0, w: ${q.w}}`).replace(/m_Father: \{fileID: \d+\}/,'m_Father: {fileID: '+detailTransform+'}');appended.push(raw);}roots.push(ids.get(t.id));accents++;}}}
const go=`--- !u!1 &${detailGo}\nGameObject:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  serializedVersion: 6\n  m_Component:\n  - component: {fileID: ${detailTransform}}\n  m_Layer: 0\n  m_Name: A3_A1_Decoration_DetailPass\n  m_TagString: Untagged\n  m_IsActive: 1\n`;
const tr=`--- !u!4 &${detailTransform}\nTransform:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: ${detailGo}}\n  serializedVersion: 2\n  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 0, y: 0, z: 0}\n  m_LocalScale: {x: 1, y: 1, z: 1}\n  m_ConstrainProportionsScale: 0\n  m_Children:\n${roots.map(id=>'  - {fileID: '+id+'}\n').join('')}  m_Father: {fileID: ${art.id}}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n`;
let output=source.replace(art.raw,art.raw.replace('  m_Children:\n','  m_Children:\n  - {fileID: '+detailTransform+'}\n'));
const sceneRoots=docs.find(d=>d.type==='1660057539');output=output.replace(sceneRoots.raw,go+tr+appended.join('')+sceneRoots.raw);
const result=parse(output),resultMap=new Map(result.map(d=>[d.id,d]));if(resultMap.size!==result.length)throw Error('Duplicate ids');
for(const d of result)for(const m of d.raw.matchAll(/\{fileID: (\d+)\}/g))if(m[1]!=='0'&&!resultMap.has(m[1]))throw Error('Dangling reference '+m[1]);
for(const d of docs)if(d.id!==art.id&&resultMap.get(d.id).raw!==d.raw)throw Error('Existing content changed '+d.id);
for(const d of appended)if(/^--- !u!(64|65|135|136) /m.test(d)&&/m_Enabled: 1/.test(d))throw Error('Enabled decoration collider');
fs.copyFileSync(scenePath,'_to_delete/A3-review/A3-before-detail-pass.unity.txt');fs.writeFileSync(scenePath,output);
fs.writeFileSync('_to_delete/A3-review/detail-pass.json',JSON.stringify({addedFurniture:audit.length,addedWallAccents:accents,originalSceneDocumentsPreserved:docs.length-1,newDanglingReferences:0,minimumFurnitureDistanceFromCenter:Math.min(...audit.map(a=>a.inner)),audit},null,2));
console.log(JSON.stringify({addedFurniture:audit.length,addedWallAccents:accents,preservedDocuments:docs.length-1,totalDocuments:result.length}));
